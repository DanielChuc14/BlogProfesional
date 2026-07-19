using BlogPlatform.Application.DTOs.Lists;
using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Application.Interfaces;
using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Entities.Content;
using BlogPlatform.Domain.Interfaces;
using BlogPlatform.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Infrastructure.Services;

public class BlogListService(IUnitOfWork uow) : IBlogListService
{
    public async Task<ResultModel<List<BlogListSummaryDto>>> GetByProfileAsync(
        string username, bool includePrivate, CancellationToken ct = default)
    {
        var profile = await uow.BlogProfiles.Query()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.User.UserName == username, ct);

        if (profile is null)
            return ResultModel<List<BlogListSummaryDto>>.NotFound("User not found.");

        var q = uow.BlogLists.Query()
            .Where(l => l.BlogProfileId == profile.Id);

        if (!includePrivate)
            q = q.Where(l => l.IsPublic);

        var lists = await q.OrderBy(l => l.Order).ToListAsync(ct);

        var result = new List<BlogListSummaryDto>();
        foreach (var list in lists)
        {
            var itemCount = await uow.BlogListItems.Query()
                .CountAsync(i => i.BlogListId == list.Id, ct);

            result.Add(new BlogListSummaryDto
            {
                Id = list.Id,
                Title = list.Title,
                Description = list.Description,
                Slug = list.Slug,
                IsPublic = list.IsPublic,
                CoverImageUrl = list.CoverImageUrl,
                ItemCount = itemCount,
                Order = list.Order,
                CreatedAt = list.CreatedAt
            });
        }

        return ResultModel<List<BlogListSummaryDto>>.Ok(result);
    }

    public async Task<ResultModel<BlogListDto>> GetBySlugAsync(
        string username, string slug, CancellationToken ct = default)
    {
        var list = await uow.BlogLists.Query()
            .Include(l => l.BlogProfile).ThenInclude(p => p.User)
            .Include(l => l.Items).ThenInclude(i => i.Post).ThenInclude(p => p.BlogProfile).ThenInclude(bp => bp.User)
            .Include(l => l.Items).ThenInclude(i => i.Post).ThenInclude(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(l => l.BlogProfile.User.UserName == username && l.Slug == slug, ct);

        if (list is null)
            return ResultModel<BlogListDto>.NotFound("List not found.");

        if (!list.IsPublic)
            return ResultModel<BlogListDto>.NotFound("List not found.");

        return ResultModel<BlogListDto>.Ok(MapToDto(list));
    }

    public async Task<ResultModel<BlogListDto>> CreateAsync(
        Guid userId, CreateBlogListRequest request, CancellationToken ct = default)
    {
        var profile = await uow.BlogProfiles.Query()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
            return ResultModel<BlogListDto>.NotFound("Blog profile not found.");

        var baseSlug = SlugHelper.Generate(request.Title);
        var existingSlugs = await uow.BlogLists.Query()
            .Where(l => l.BlogProfileId == profile.Id && l.Slug.StartsWith(baseSlug))
            .Select(l => l.Slug)
            .ToListAsync(ct);

        var slug = SlugHelper.MakeUnique(baseSlug, existingSlugs);

        var list = new BlogList
        {
            BlogProfileId = profile.Id,
            Title = request.Title,
            Description = request.Description,
            Slug = slug,
            IsPublic = request.IsPublic,
            CoverImageUrl = request.CoverImageUrl,
            Order = request.Order
        };

        await uow.BlogLists.AddAsync(list, ct);
        await uow.SaveChangesAsync(ct);

        var created = await LoadList(list.Id, ct);
        return ResultModel<BlogListDto>.Created(MapToDto(created!));
    }

    public async Task<ResultModel<BlogListDto>> UpdateAsync(
        Guid userId, Guid listId, UpdateBlogListRequest request, CancellationToken ct = default)
    {
        var list = await uow.BlogLists.Query()
            .Include(l => l.BlogProfile)
            .FirstOrDefaultAsync(l => l.Id == listId, ct);

        if (list is null)
            return ResultModel<BlogListDto>.NotFound("List not found.");

        if (list.BlogProfile.UserId != userId)
            return ResultModel<BlogListDto>.Forbidden("You do not own this list.");

        if (request.Title is not null) list.Title = request.Title;
        if (request.Description is not null) list.Description = request.Description;
        if (request.IsPublic.HasValue) list.IsPublic = request.IsPublic.Value;
        if (request.CoverImageUrl is not null) list.CoverImageUrl = request.CoverImageUrl;
        if (request.Order.HasValue) list.Order = request.Order.Value;
        list.UpdatedAt = DateTime.UtcNow;

        uow.BlogLists.Update(list);
        await uow.SaveChangesAsync(ct);

        var updated = await LoadList(list.Id, ct);
        return ResultModel<BlogListDto>.Ok(MapToDto(updated!));
    }

    public async Task<ResultModel> DeleteAsync(Guid userId, Guid listId, CancellationToken ct = default)
    {
        var list = await uow.BlogLists.Query()
            .Include(l => l.BlogProfile)
            .FirstOrDefaultAsync(l => l.Id == listId, ct);

        if (list is null)
            return ResultModel.NotFound("List not found.");

        if (list.BlogProfile.UserId != userId)
            return ResultModel.Forbidden("You do not own this list.");

        uow.BlogLists.Remove(list);
        await uow.SaveChangesAsync(ct);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel> AddPostAsync(
        Guid userId, Guid listId, AddPostToListRequest request, CancellationToken ct = default)
    {
        var list = await uow.BlogLists.Query()
            .Include(l => l.BlogProfile)
            .FirstOrDefaultAsync(l => l.Id == listId, ct);

        if (list is null)
            return ResultModel.NotFound("List not found.");

        if (list.BlogProfile.UserId != userId)
            return ResultModel.Forbidden("You do not own this list.");

        var post = await uow.Posts.GetByIdAsync(request.PostId, ct);
        if (post is null)
            return ResultModel.NotFound("Post not found.");

        var exists = await uow.BlogListItems.Query()
            .AnyAsync(i => i.BlogListId == listId && i.PostId == request.PostId, ct);

        if (exists)
            return ResultModel.Conflict("Post is already in this list.");

        await uow.BlogListItems.AddAsync(new BlogListItem
        {
            BlogListId = listId,
            PostId = request.PostId,
            Order = request.Order
        }, ct);

        await uow.SaveChangesAsync(ct);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel> RemovePostAsync(
        Guid userId, Guid listId, Guid postId, CancellationToken ct = default)
    {
        var list = await uow.BlogLists.Query()
            .Include(l => l.BlogProfile)
            .FirstOrDefaultAsync(l => l.Id == listId, ct);

        if (list is null)
            return ResultModel.NotFound("List not found.");

        if (list.BlogProfile.UserId != userId)
            return ResultModel.Forbidden("You do not own this list.");

        var item = await uow.BlogListItems.Query()
            .FirstOrDefaultAsync(i => i.BlogListId == listId && i.PostId == postId, ct);

        if (item is null)
            return ResultModel.NotFound("Post not found in this list.");

        uow.BlogListItems.Remove(item);
        await uow.SaveChangesAsync(ct);
        return ResultModel.NoContent();
    }

    private async Task<BlogList?> LoadList(Guid listId, CancellationToken ct)
    {
        return await uow.BlogLists.Query()
            .Include(l => l.Items).ThenInclude(i => i.Post).ThenInclude(p => p.BlogProfile).ThenInclude(bp => bp.User)
            .Include(l => l.Items).ThenInclude(i => i.Post).ThenInclude(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(l => l.Id == listId, ct);
    }

    private static BlogListDto MapToDto(BlogList list) => new()
    {
        Id = list.Id,
        Title = list.Title,
        Description = list.Description,
        Slug = list.Slug,
        IsPublic = list.IsPublic,
        CoverImageUrl = list.CoverImageUrl,
        Order = list.Order,
        Posts = list.Items
            .OrderBy(i => i.Order)
            .Select(i => new PostSummaryDto
            {
                Id = i.Post.Id,
                Title = i.Post.Title,
                Slug = i.Post.Slug,
                Excerpt = i.Post.Excerpt,
                CoverImageUrl = i.Post.CoverImageUrl,
                Status = i.Post.Status.ToString(),
                PublishedAt = i.Post.PublishedAt,
                ReadTimeMinutes = i.Post.ReadTimeMinutes,
                ViewCount = i.Post.ViewCount,
                LikesCount = i.Post.LikesCount,
                CommentsCount = i.Post.CommentsCount,
                IsAdultContent = i.Post.IsAdultContent,
                IsFeatured = i.Post.IsFeatured,
                Tags = i.Post.PostTags.Select(pt => new PostTagDto { Id = pt.Tag.Id, Name = pt.Tag.Name, Slug = pt.Tag.Slug }).ToList(),
                Author = new AuthorDto
                {
                    UserId = i.Post.BlogProfile.UserId,
                    Username = i.Post.BlogProfile.User.UserName ?? string.Empty,
                    DisplayName = i.Post.BlogProfile.User.DisplayName,
                    AvatarUrl = i.Post.BlogProfile.User.AvatarUrl
                },
                CreatedAt = i.Post.CreatedAt
            })
            .ToList(),
        CreatedAt = list.CreatedAt,
        UpdatedAt = list.UpdatedAt
    };
}
