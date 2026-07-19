using System.Text;
using BlogPlatform.Application.DTOs.Common;
using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Application.Interfaces;
using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Entities.Content;
using BlogPlatform.Domain.Enums;
using BlogPlatform.Domain.Interfaces;
using BlogPlatform.Infrastructure.Helpers;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NpgsqlTypes;

namespace BlogPlatform.Infrastructure.Services;

public class PostService(
    IUnitOfWork uow,
    IValidator<CreatePostRequest> createValidator,
    IValidator<UpdatePostRequest> updateValidator,
    ILogger<PostService> logger) : IPostService
{
    public async Task<ResultModel<CursorPagedResult<PostSummaryDto>>> GetFeedAsync(PostFeedQuery query, CancellationToken ct = default)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        var q = uow.Posts.Query()
            .Include(p => p.BlogProfile).ThenInclude(bp => bp.User)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .Where(p => p.Status == PostStatus.Published);

        if (!string.IsNullOrWhiteSpace(query.TagSlug))
            q = q.Where(p => p.PostTags.Any(pt => pt.Tag.Slug == query.TagSlug));

        if (!string.IsNullOrWhiteSpace(query.AuthorUsername))
            q = q.Where(p => p.BlogProfile.User.UserName == query.AuthorUsername);

        if (!query.IncludeAdultContent)
            q = q.Where(p => !p.IsAdultContent);

        if (!string.IsNullOrEmpty(query.Cursor))
        {
            var (cursorDate, cursorId) = DecodeCursor(query.Cursor);
            q = q.Where(p => p.PublishedAt < cursorDate ||
                              (p.PublishedAt == cursorDate && p.Id.CompareTo(cursorId) < 0));
        }

        var items = await q
            .OrderByDescending(p => p.PublishedAt)
            .ThenByDescending(p => p.Id)
            .Take(pageSize + 1)
            .ToListAsync(ct);

        var hasMore = items.Count > pageSize;
        if (hasMore) items.RemoveAt(items.Count - 1);

        string? nextCursor = null;
        if (hasMore && items.Count > 0)
        {
            var last = items[^1];
            nextCursor = EncodeCursor(last.PublishedAt ?? last.CreatedAt, last.Id);
        }

        return ResultModel<CursorPagedResult<PostSummaryDto>>.Ok(new CursorPagedResult<PostSummaryDto>
        {
            Items = items.Select(MapToSummary).ToList(),
            NextCursor = nextCursor,
            HasMore = hasMore
        });
    }

    public async Task<ResultModel<PostDetailDto>> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var post = await uow.Posts.Query()
            .Include(p => p.BlogProfile).ThenInclude(bp => bp.User)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.Seo)
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Slug == slug, ct);

        if (post is null)
            return ResultModel<PostDetailDto>.NotFound("Post not found.");

        if (post.Status != PostStatus.Published)
            return ResultModel<PostDetailDto>.NotFound("Post not found.");

        return ResultModel<PostDetailDto>.Ok(MapToDetail(post));
    }

    public async Task<ResultModel<PostDetailDto>> GetByIdAsync(Guid userId, Guid postId, CancellationToken ct = default)
    {
        var post = await uow.Posts.Query()
            .Include(p => p.BlogProfile).ThenInclude(bp => bp.User)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.Seo)
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Id == postId, ct);

        if (post is null)
            return ResultModel<PostDetailDto>.NotFound("Post not found.");

        if (post.BlogProfile.UserId != userId)
            return ResultModel<PostDetailDto>.Forbidden("Access denied.");

        return ResultModel<PostDetailDto>.Ok(MapToDetail(post));
    }

    public async Task<ResultModel<PostDetailDto>> CreateAsync(Guid userId, CreatePostRequest request, CancellationToken ct = default)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);

        var wordCheck = await CheckRestrictedWordsAsync(
            $"{request.Title} {request.Excerpt} {request.Body}", ct);
        if (wordCheck is not null)
            return ResultModel<PostDetailDto>.BadRequest(wordCheck);

        var profile = await uow.BlogProfiles.Query()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
            return ResultModel<PostDetailDto>.NotFound("Blog profile not found.");

        var baseSlug = SlugHelper.Generate(request.Title);
        var existingSlugs = await uow.Posts.Query()
            .Where(p => p.Slug.StartsWith(baseSlug))
            .Select(p => p.Slug)
            .ToListAsync(ct);

        var slug = SlugHelper.MakeUnique(baseSlug, existingSlugs);

        var post = new Post
        {
            BlogProfileId = profile.Id,
            Title = request.Title,
            Content = request.Body,
            Slug = slug,
            Excerpt = request.Excerpt,
            CoverImageUrl = request.CoverImageUrl,
            Status = PostStatus.Draft,
            ReadTimeMinutes = CalculateReadTime(request.Body),
            IsAdultContent = request.IsAdultContent
        };

        await uow.Posts.AddAsync(post, ct);
        await uow.SaveChangesAsync(ct);

        await SyncTagsAsync(post.Id, request.TagIds, ct);

        if (request.Seo is not null)
        {
            await uow.PostSeos.AddAsync(new PostSeo
            {
                PostId = post.Id,
                MetaTitle = request.Seo.MetaTitle,
                MetaDescription = request.Seo.MetaDescription,
                OgImageUrl = request.Seo.OgImageUrl,
                CanonicalUrl = request.Seo.CanonicalUrl
            }, ct);
            await uow.SaveChangesAsync(ct);
        }

        logger.LogInformation("Post created: {PostId} by user {UserId}", post.Id, userId);

        var created = await LoadFullPost(post.Id, ct);
        return ResultModel<PostDetailDto>.Created(MapToDetail(created!));
    }

    public async Task<ResultModel<PostDetailDto>> UpdateAsync(Guid userId, Guid postId, UpdatePostRequest request, CancellationToken ct = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, ct);

        var checkContent = $"{request.Title} {request.Excerpt} {request.Body}";
        var wordCheck = await CheckRestrictedWordsAsync(checkContent, ct);
        if (wordCheck is not null)
            return ResultModel<PostDetailDto>.BadRequest(wordCheck);

        var post = await uow.Posts.Query()
            .Include(p => p.BlogProfile)
            .Include(p => p.PostTags)
            .Include(p => p.Seo)
            .FirstOrDefaultAsync(p => p.Id == postId, ct);

        if (post is null)
            return ResultModel<PostDetailDto>.NotFound("Post not found.");

        if (post.BlogProfile.UserId != userId)
            return ResultModel<PostDetailDto>.Forbidden("You do not own this post.");

        if (request.Title is not null)
        {
            post.Title = request.Title;
            post.ReadTimeMinutes = CalculateReadTime(request.Body ?? post.Content);
        }

        if (request.Body is not null)
        {
            post.Content = request.Body;
            post.ReadTimeMinutes = CalculateReadTime(request.Body);
        }

        if (request.Excerpt is not null) post.Excerpt = request.Excerpt;
        if (request.CoverImageUrl is not null) post.CoverImageUrl = request.CoverImageUrl;
        if (request.IsAdultContent.HasValue) post.IsAdultContent = request.IsAdultContent.Value;

        if (request.TagIds is not null)
            await SyncTagsAsync(post.Id, request.TagIds, ct);

        if (request.Seo is not null)
        {
            if (post.Seo is not null)
            {
                post.Seo.MetaTitle = request.Seo.MetaTitle;
                post.Seo.MetaDescription = request.Seo.MetaDescription;
                post.Seo.OgImageUrl = request.Seo.OgImageUrl;
                post.Seo.CanonicalUrl = request.Seo.CanonicalUrl;
                uow.PostSeos.Update(post.Seo);
            }
            else
            {
                await uow.PostSeos.AddAsync(new PostSeo
                {
                    PostId = post.Id,
                    MetaTitle = request.Seo.MetaTitle,
                    MetaDescription = request.Seo.MetaDescription,
                    OgImageUrl = request.Seo.OgImageUrl,
                    CanonicalUrl = request.Seo.CanonicalUrl
                }, ct);
            }
        }

        uow.Posts.Update(post);
        await uow.SaveChangesAsync(ct);

        var updated = await LoadFullPost(post.Id, ct);
        return ResultModel<PostDetailDto>.Ok(MapToDetail(updated!));
    }

    public async Task<ResultModel> DeleteAsync(Guid userId, Guid postId, CancellationToken ct = default)
    {
        var post = await uow.Posts.Query()
            .Include(p => p.BlogProfile)
            .FirstOrDefaultAsync(p => p.Id == postId, ct);

        if (post is null)
            return ResultModel.NotFound("Post not found.");

        if (post.BlogProfile.UserId != userId)
            return ResultModel.Forbidden("You do not own this post.");

        uow.Posts.Remove(post);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Post deleted: {PostId}", postId);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel<PostDetailDto>> PublishAsync(Guid userId, Guid postId, CancellationToken ct = default)
    {
        var post = await uow.Posts.Query()
            .Include(p => p.BlogProfile)
            .FirstOrDefaultAsync(p => p.Id == postId, ct);

        if (post is null)
            return ResultModel<PostDetailDto>.NotFound("Post not found.");

        if (post.BlogProfile.UserId != userId)
            return ResultModel<PostDetailDto>.Forbidden("You do not own this post.");

        post.Status = PostStatus.Published;
        post.PublishedAt = DateTime.UtcNow;
        post.ScheduledAt = null;

        uow.Posts.Update(post);
        await uow.SaveChangesAsync(ct);

        var updated = await LoadFullPost(post.Id, ct);
        return ResultModel<PostDetailDto>.Ok(MapToDetail(updated!));
    }

    public async Task<ResultModel<PostDetailDto>> ScheduleAsync(Guid userId, Guid postId, SchedulePostRequest request, CancellationToken ct = default)
    {
        if (request.ScheduledAt <= DateTime.UtcNow)
            return ResultModel<PostDetailDto>.BadRequest("Scheduled time must be in the future.");

        var post = await uow.Posts.Query()
            .Include(p => p.BlogProfile)
            .FirstOrDefaultAsync(p => p.Id == postId, ct);

        if (post is null)
            return ResultModel<PostDetailDto>.NotFound("Post not found.");

        if (post.BlogProfile.UserId != userId)
            return ResultModel<PostDetailDto>.Forbidden("You do not own this post.");

        post.Status = PostStatus.Scheduled;
        post.ScheduledAt = request.ScheduledAt;

        uow.Posts.Update(post);
        await uow.SaveChangesAsync(ct);

        var updated = await LoadFullPost(post.Id, ct);
        return ResultModel<PostDetailDto>.Ok(MapToDetail(updated!));
    }

    public async Task<ResultModel<PostDetailDto>> ArchiveAsync(Guid userId, Guid postId, CancellationToken ct = default)
    {
        var post = await uow.Posts.Query()
            .Include(p => p.BlogProfile)
            .FirstOrDefaultAsync(p => p.Id == postId, ct);

        if (post is null)
            return ResultModel<PostDetailDto>.NotFound("Post not found.");

        if (post.BlogProfile.UserId != userId)
            return ResultModel<PostDetailDto>.Forbidden("You do not own this post.");

        post.Status = PostStatus.Archived;

        uow.Posts.Update(post);
        await uow.SaveChangesAsync(ct);

        var updated = await LoadFullPost(post.Id, ct);
        return ResultModel<PostDetailDto>.Ok(MapToDetail(updated!));
    }

    public async Task<ResultModel<CursorPagedResult<PostSummaryDto>>> SearchAsync(SearchQuery query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query.Term))
            return ResultModel<CursorPagedResult<PostSummaryDto>>.BadRequest("Search term is required.");

        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        var sanitized = query.Term.Trim();
        var tsQuery = EF.Functions.PlainToTsQuery("english", sanitized);

        var q = uow.Posts.Query()
            .Include(p => p.BlogProfile).ThenInclude(bp => bp.User)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .Where(p => p.Status == PostStatus.Published)
            .Where(p => EF.Property<NpgsqlTsVector>(p, "SearchVector").Matches(tsQuery));

        if (!string.IsNullOrEmpty(query.Cursor))
        {
            var (cursorDate, cursorId) = DecodeCursor(query.Cursor);
            q = q.Where(p => p.PublishedAt < cursorDate ||
                              (p.PublishedAt == cursorDate && p.Id.CompareTo(cursorId) < 0));
        }

        var items = await q
            .OrderByDescending(p => p.PublishedAt)
            .ThenByDescending(p => p.Id)
            .Take(pageSize + 1)
            .ToListAsync(ct);

        var hasMore = items.Count > pageSize;
        if (hasMore) items.RemoveAt(items.Count - 1);

        string? nextCursor = null;
        if (hasMore && items.Count > 0)
        {
            var last = items[^1];
            nextCursor = EncodeCursor(last.PublishedAt ?? last.CreatedAt, last.Id);
        }

        return ResultModel<CursorPagedResult<PostSummaryDto>>.Ok(new CursorPagedResult<PostSummaryDto>
        {
            Items = items.Select(MapToSummary).ToList(),
            NextCursor = nextCursor,
            HasMore = hasMore
        });
    }

    public async Task<ResultModel> LikePostAsync(Guid userId, Guid postId, CancellationToken ct = default)
    {
        var post = await uow.Posts.GetByIdAsync(postId, ct);
        if (post is null) return ResultModel.NotFound("Post not found.");

        var existing = await uow.PostLikes.Query()
            .FirstOrDefaultAsync(pl => pl.PostId == postId && pl.UserId == userId, ct);

        if (existing is not null) return ResultModel.Conflict("Already liked.");

        await uow.PostLikes.AddAsync(new PostLike { PostId = postId, UserId = userId }, ct);
        post.LikesCount++;
        uow.Posts.Update(post);
        await uow.SaveChangesAsync(ct);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel> UnlikePostAsync(Guid userId, Guid postId, CancellationToken ct = default)
    {
        var like = await uow.PostLikes.Query()
            .FirstOrDefaultAsync(pl => pl.PostId == postId && pl.UserId == userId, ct);

        if (like is null) return ResultModel.NotFound("Like not found.");

        uow.PostLikes.Remove(like);

        var post = await uow.Posts.GetByIdAsync(postId, ct);
        if (post is not null)
        {
            post.LikesCount = Math.Max(0, post.LikesCount - 1);
            uow.Posts.Update(post);
        }

        await uow.SaveChangesAsync(ct);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel<PostDetailDto>> ChangeSlugAsync(Guid userId, Guid postId, string newSlug, CancellationToken ct = default)
    {
        var post = await uow.Posts.Query()
            .Include(p => p.BlogProfile)
            .FirstOrDefaultAsync(p => p.Id == postId, ct);

        if (post is null)
            return ResultModel<PostDetailDto>.NotFound("Post not found.");

        if (post.BlogProfile.UserId != userId)
            return ResultModel<PostDetailDto>.Forbidden("You do not own this post.");

        var slug = SlugHelper.Generate(newSlug);

        var slugExists = await uow.Posts.Query()
            .AnyAsync(p => p.Slug == slug && p.Id != postId, ct);

        if (slugExists)
            return ResultModel<PostDetailDto>.Conflict("Slug is already in use.");

        if (post.Slug != slug)
        {
            var redirect = await uow.SlugRedirects.Query()
                .FirstOrDefaultAsync(r => r.OldSlug == post.Slug, ct);

            if (redirect is null)
            {
                await uow.SlugRedirects.AddAsync(new SlugRedirect
                {
                    OldSlug = post.Slug,
                    PostId = post.Id
                }, ct);
            }

            post.Slug = slug;
            uow.Posts.Update(post);
            await uow.SaveChangesAsync(ct);
        }

        var updated = await LoadFullPost(post.Id, ct);
        return ResultModel<PostDetailDto>.Ok(MapToDetail(updated!));
    }

    private async Task SyncTagsAsync(Guid postId, IReadOnlyList<Guid> tagIds, CancellationToken ct)
    {
        var existingPostTags = await uow.PostTags.Query()
            .Where(pt => pt.PostId == postId)
            .ToListAsync(ct);

        foreach (var pt in existingPostTags)
        {
            uow.PostTags.Remove(pt);
            var tag = await uow.Tags.GetByIdAsync(pt.TagId, ct);
            if (tag is not null && tag.PostCount > 0)
            {
                tag.PostCount--;
                uow.Tags.Update(tag);
            }
        }

        foreach (var tagId in tagIds.Distinct())
        {
            var tag = await uow.Tags.GetByIdAsync(tagId, ct);
            if (tag is null) continue;

            await uow.PostTags.AddAsync(new PostTag { PostId = postId, TagId = tag.Id }, ct);
            tag.PostCount++;
            uow.Tags.Update(tag);
        }

        await uow.SaveChangesAsync(ct);
    }

    private async Task<Post?> LoadFullPost(Guid postId, CancellationToken ct)
    {
        return await uow.Posts.Query()
            .Include(p => p.BlogProfile).ThenInclude(bp => bp.User)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.Seo)
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Id == postId, ct);
    }

    private static int CalculateReadTime(string content)
    {
        var wordCount = content.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Max(1, (int)Math.Ceiling(wordCount / 200.0));
    }

    private static string EncodeCursor(DateTime date, Guid id)
    {
        var raw = $"{date:O}|{id}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }

    private static (DateTime date, Guid id) DecodeCursor(string cursor)
    {
        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = raw.Split('|');
            return (DateTime.Parse(parts[0], null, System.Globalization.DateTimeStyles.RoundtripKind), Guid.Parse(parts[1]));
        }
        catch
        {
            return (DateTime.UtcNow, Guid.Empty);
        }
    }

    private async Task<string?> CheckRestrictedWordsAsync(string content, CancellationToken ct)
    {
        var restrictedWords = await uow.RestrictedWords.Query().ToListAsync(ct);
        foreach (var rw in restrictedWords)
        {
            bool found;
            if (rw.IsRegex)
            {
                try { found = System.Text.RegularExpressions.Regex.IsMatch(content, rw.Phrase, System.Text.RegularExpressions.RegexOptions.IgnoreCase); }
                catch { found = false; }
            }
            else
            {
                found = content.Contains(rw.Phrase, StringComparison.OrdinalIgnoreCase);
            }
            if (found)
                return $"Content contains a restricted word or phrase: \"{rw.Phrase}\".";
        }
        return null;
    }

    private static PostSummaryDto MapToSummary(Post post) => new()
    {
        Id = post.Id,
        Title = post.Title,
        Slug = post.Slug,
        Excerpt = post.Excerpt,
        CoverImageUrl = post.CoverImageUrl,
        Status = post.Status.ToString(),
        PublishedAt = post.PublishedAt,
        ReadTimeMinutes = post.ReadTimeMinutes,
        ViewCount = post.ViewCount,
        LikesCount = post.LikesCount,
        CommentsCount = post.CommentsCount,
        IsAdultContent = post.IsAdultContent,
        IsFeatured = post.IsFeatured,
        Tags = post.PostTags.Select(pt => new PostTagDto { Id = pt.Tag.Id, Name = pt.Tag.Name, Slug = pt.Tag.Slug }).ToList(),
        Author = new AuthorDto
        {
            UserId = post.BlogProfile.UserId,
            Username = post.BlogProfile.User.UserName ?? string.Empty,
            DisplayName = post.BlogProfile.User.DisplayName,
            AvatarUrl = post.BlogProfile.User.AvatarUrl
        },
        CreatedAt = post.CreatedAt
    };

    private static PostDetailDto MapToDetail(Post post) => new()
    {
        Id = post.Id,
        Title = post.Title,
        Slug = post.Slug,
        Body = post.Content,
        Excerpt = post.Excerpt,
        CoverImageUrl = post.CoverImageUrl,
        Status = post.Status.ToString(),
        PublishedAt = post.PublishedAt,
        ScheduledAt = post.ScheduledAt,
        ReadTimeMinutes = post.ReadTimeMinutes,
        ViewCount = post.ViewCount,
        LikesCount = post.LikesCount,
        CommentsCount = post.CommentsCount,
        IsAdultContent = post.IsAdultContent,
        Tags = post.PostTags.Select(pt => new PostTagDto { Id = pt.Tag.Id, Name = pt.Tag.Name, Slug = pt.Tag.Slug }).ToList(),
        Author = new AuthorDto
        {
            UserId = post.BlogProfile.UserId,
            Username = post.BlogProfile.User.UserName ?? string.Empty,
            DisplayName = post.BlogProfile.User.DisplayName,
            AvatarUrl = post.BlogProfile.User.AvatarUrl
        },
        Seo = post.Seo is null ? null : new PostSeoDto
        {
            MetaTitle = post.Seo.MetaTitle,
            MetaDescription = post.Seo.MetaDescription,
            OgImageUrl = post.Seo.OgImageUrl,
            CanonicalUrl = post.Seo.CanonicalUrl
        },
        Media = post.Media
            .OrderBy(m => m.SortOrder)
            .Select(m => new PostMediaDto
            {
                Id = m.Id,
                Url = m.Url,
                Type = m.Type.ToString(),
                Caption = m.Caption,
                SortOrder = m.SortOrder
            })
            .ToList(),
        CreatedAt = post.CreatedAt,
        UpdatedAt = post.UpdatedAt
    };
}
