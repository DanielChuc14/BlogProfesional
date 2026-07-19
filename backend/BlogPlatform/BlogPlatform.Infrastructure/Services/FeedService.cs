using System.Text;
using BlogPlatform.Application.DTOs.Common;
using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Application.Interfaces;
using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Enums;
using BlogPlatform.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Infrastructure.Services;

public class FeedService(IUnitOfWork uow) : IFeedService
{
    public async Task<ResultModel<CursorPagedResult<PostSummaryDto>>> GetPersonalizedFeedAsync(
        Guid userId, string? cursor, int pageSize, bool includeAdultContent = false, CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);

        var followedProfileIds = await uow.Follows.Query()
            .Where(f => f.FollowerId == userId)
            .Select(f => f.BlogProfileId)
            .ToListAsync(ct);

        var userWordFilters = await uow.UserWordFilters.Query()
            .Where(f => f.UserId == userId)
            .Select(f => f.Word.ToLower())
            .ToListAsync(ct);

        var q = uow.Posts.Query()
            .Include(p => p.BlogProfile).ThenInclude(bp => bp.User)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .Where(p => p.Status == PostStatus.Published && followedProfileIds.Contains(p.BlogProfileId));

        if (!includeAdultContent)
            q = q.Where(p => !p.IsAdultContent);

        if (userWordFilters.Count > 0)
        {
            q = q.Where(p =>
                !userWordFilters.Any(w =>
                    (p.Title != null && p.Title.ToLower().Contains(w)) ||
                    (p.Excerpt != null && p.Excerpt.ToLower().Contains(w))));
        }

        if (!string.IsNullOrEmpty(cursor))
        {
            var (cursorDate, cursorId) = DecodeCursor(cursor);
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

    private static PostSummaryDto MapToSummary(Domain.Entities.Content.Post post) => new()
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
}
