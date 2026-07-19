using BlogPlatform.Application.DTOs.Community;
using BlogPlatform.Application.Interfaces;
using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Entities.Content;
using BlogPlatform.Domain.Enums;
using BlogPlatform.Domain.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Infrastructure.Services;

public class CommentService(
    IUnitOfWork uow,
    IValidator<CreateCommentRequest> createValidator,
    IValidator<UpdateCommentRequest> updateValidator,
    ILogger<CommentService> logger) : ICommentService
{
    public async Task<ResultModel<IReadOnlyList<CommentDto>>> GetByPostAsync(Guid postId, CancellationToken ct = default)
    {
        var post = await uow.Posts.GetByIdAsync(postId, ct);
        if (post is null)
            return ResultModel<IReadOnlyList<CommentDto>>.NotFound("Post not found.");

        var comments = await uow.Comments.Query()
            .Include(c => c.Author)
            .Include(c => c.Replies).ThenInclude(r => r.Author)
            .Where(c => c.PostId == postId && c.ParentId == null)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);

        return ResultModel<IReadOnlyList<CommentDto>>.Ok(comments.Select(MapToDto).ToList());
    }

    public async Task<ResultModel<CommentDto>> CreateAsync(Guid userId, Guid postId, CreateCommentRequest request, CancellationToken ct = default)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);

        var post = await uow.Posts.GetByIdAsync(postId, ct);
        if (post is null)
            return ResultModel<CommentDto>.NotFound("Post not found.");

        if (request.ParentId.HasValue)
        {
            var parent = await uow.Comments.GetByIdAsync(request.ParentId.Value, ct);
            if (parent is null || parent.PostId != postId)
                return ResultModel<CommentDto>.BadRequest("Parent comment not found.");
        }

        var comment = new Comment
        {
            PostId = postId,
            AuthorId = userId,
            ParentId = request.ParentId,
            Body = request.Body
        };

        await uow.Comments.AddAsync(comment, ct);

        post.CommentsCount++;
        uow.Posts.Update(post);

        await uow.SaveChangesAsync(ct);

        // Notify post author if different from commenter
        var profile = await uow.BlogProfiles.Query()
            .FirstOrDefaultAsync(bp => bp.Id == post.BlogProfileId, ct);

        if (profile is not null && profile.UserId != userId)
        {
            await uow.Notifications.AddAsync(new Notification
            {
                RecipientId = profile.UserId,
                ActorId = userId,
                Type = NotificationType.NewComment,
                PostId = postId,
                CommentId = comment.Id
            }, ct);
            await uow.SaveChangesAsync(ct);
        }

        logger.LogInformation("Comment {CommentId} created on post {PostId}", comment.Id, postId);

        var created = await LoadComment(comment.Id, ct);
        return ResultModel<CommentDto>.Created(MapToDto(created!));
    }

    public async Task<ResultModel<CommentDto>> UpdateAsync(Guid userId, Guid commentId, UpdateCommentRequest request, CancellationToken ct = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, ct);

        var comment = await uow.Comments.GetByIdAsync(commentId, ct);
        if (comment is null || comment.IsDeleted)
            return ResultModel<CommentDto>.NotFound("Comment not found.");

        if (comment.AuthorId != userId)
            return ResultModel<CommentDto>.Forbidden("You do not own this comment.");

        comment.Body = request.Body;
        uow.Comments.Update(comment);
        await uow.SaveChangesAsync(ct);

        var updated = await LoadComment(comment.Id, ct);
        return ResultModel<CommentDto>.Ok(MapToDto(updated!));
    }

    public async Task<ResultModel> DeleteAsync(Guid userId, Guid commentId, CancellationToken ct = default)
    {
        var comment = await uow.Comments.Query()
            .Include(c => c.Post).ThenInclude(p => p.BlogProfile)
            .FirstOrDefaultAsync(c => c.Id == commentId, ct);

        if (comment is null || comment.IsDeleted)
            return ResultModel.NotFound("Comment not found.");

        var isOwner = comment.AuthorId == userId;
        var isPostOwner = comment.Post.BlogProfile.UserId == userId;

        if (!isOwner && !isPostOwner)
            return ResultModel.Forbidden("You cannot delete this comment.");

        comment.IsDeleted = true;
        comment.Body = string.Empty;
        uow.Comments.Update(comment);

        comment.Post.CommentsCount = Math.Max(0, comment.Post.CommentsCount - 1);
        uow.Posts.Update(comment.Post);

        await uow.SaveChangesAsync(ct);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel> LikeAsync(Guid userId, Guid commentId, CancellationToken ct = default)
    {
        var comment = await uow.Comments.GetByIdAsync(commentId, ct);
        if (comment is null || comment.IsDeleted)
            return ResultModel.NotFound("Comment not found.");

        var existing = await uow.CommentLikes.Query()
            .FirstOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserId == userId, ct);

        if (existing is not null)
            return ResultModel.Conflict("Already liked.");

        await uow.CommentLikes.AddAsync(new CommentLike { CommentId = commentId, UserId = userId }, ct);
        comment.LikesCount++;
        uow.Comments.Update(comment);
        await uow.SaveChangesAsync(ct);

        return ResultModel.NoContent();
    }

    public async Task<ResultModel> UnlikeAsync(Guid userId, Guid commentId, CancellationToken ct = default)
    {
        var like = await uow.CommentLikes.Query()
            .FirstOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserId == userId, ct);

        if (like is null)
            return ResultModel.NotFound("Like not found.");

        uow.CommentLikes.Remove(like);

        var comment = await uow.Comments.GetByIdAsync(commentId, ct);
        if (comment is not null)
        {
            comment.LikesCount = Math.Max(0, comment.LikesCount - 1);
            uow.Comments.Update(comment);
        }

        await uow.SaveChangesAsync(ct);
        return ResultModel.NoContent();
    }

    private async Task<Comment?> LoadComment(Guid id, CancellationToken ct)
    {
        return await uow.Comments.Query()
            .Include(c => c.Author)
            .Include(c => c.Replies).ThenInclude(r => r.Author)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    private static CommentDto MapToDto(Comment c) => new()
    {
        Id = c.Id,
        PostId = c.PostId,
        ParentId = c.ParentId,
        AuthorId = c.AuthorId,
        AuthorUsername = c.Author.UserName ?? string.Empty,
        AuthorDisplayName = c.Author.DisplayName,
        AuthorAvatarUrl = c.Author.AvatarUrl,
        Body = c.IsDeleted ? "[deleted]" : c.Body,
        LikesCount = c.LikesCount,
        IsDeleted = c.IsDeleted,
        CreatedAt = c.CreatedAt,
        Replies = c.Replies
            .Where(r => !r.IsDeleted || r.Replies.Count > 0)
            .OrderBy(r => r.CreatedAt)
            .Select(MapToDto)
            .ToList()
    };
}
