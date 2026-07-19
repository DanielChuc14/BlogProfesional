using BlogPlatform.Application.DTOs.Common;
using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Application.DTOs.Tags;
using BlogPlatform.Application.Interfaces;
using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Enums;
using BlogPlatform.Domain.Interfaces;
using BlogPlatform.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace BlogPlatform.Infrastructure.Services;

public class TagService(IUnitOfWork uow) : ITagService
{
    public async Task<ResultModel<List<TagDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var tags = await uow.Tags.Query()
            .OrderByDescending(t => t.PostCount)
            .Select(t => new TagDto { Id = t.Id, Name = t.Name, Slug = t.Slug, PostsCount = t.PostCount })
            .ToListAsync(ct);

        return ResultModel<List<TagDto>>.Ok(tags);
    }

    public async Task<ResultModel<TagDto>> CreateAsync(string name, CancellationToken ct = default)
    {
        name = name.Trim();
        var slug = SlugHelper.Generate(name);

        var exists = await uow.Tags.Query().AnyAsync(t => t.Slug == slug, ct);
        if (exists) return ResultModel<TagDto>.Conflict($"Tag '{name}' already exists.");

        var tag = new Domain.Entities.Content.Tag { Name = name, Slug = slug };
        await uow.Tags.AddAsync(tag, ct);
        await uow.SaveChangesAsync(ct);

        return ResultModel<TagDto>.Created(new TagDto { Id = tag.Id, Name = tag.Name, Slug = tag.Slug, PostsCount = 0 });
    }

    public async Task<ResultModel<TagDto>> UpdateAsync(Guid id, string name, CancellationToken ct = default)
    {
        var tag = await uow.Tags.GetByIdAsync(id, ct);
        if (tag is null) return ResultModel<TagDto>.NotFound("Tag not found.");

        name = name.Trim();
        var newSlug = SlugHelper.Generate(name);

        var slugTaken = await uow.Tags.Query().AnyAsync(t => t.Slug == newSlug && t.Id != id, ct);
        if (slugTaken) return ResultModel<TagDto>.Conflict($"Tag '{name}' already exists.");

        tag.Name = name;
        tag.Slug = newSlug;
        await uow.SaveChangesAsync(ct);

        return ResultModel<TagDto>.Ok(new TagDto { Id = tag.Id, Name = tag.Name, Slug = tag.Slug, PostsCount = tag.PostCount });
    }

    public async Task<ResultModel<List<TagDto>>> AutocompleteAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return ResultModel<List<TagDto>>.Ok([]);

        var term = query.Trim().ToLower();
        var tags = await uow.Tags.Query()
            .Where(t => t.Name.ToLower().StartsWith(term))
            .OrderByDescending(t => t.PostCount)
            .Take(10)
            .Select(t => new TagDto { Id = t.Id, Name = t.Name, Slug = t.Slug, PostsCount = t.PostCount })
            .ToListAsync(ct);

        return ResultModel<List<TagDto>>.Ok(tags);
    }

    public async Task<ResultModel<CursorPagedResult<PostSummaryDto>>> GetPostsByTagAsync(
        string tagSlug, string? cursor, int pageSize, CancellationToken ct = default)
    {
        var tag = await uow.Tags.Query()
            .FirstOrDefaultAsync(t => t.Slug == tagSlug, ct);

        if (tag is null)
            return ResultModel<CursorPagedResult<PostSummaryDto>>.NotFound("Tag not found.");

        pageSize = Math.Clamp(pageSize, 1, 50);

        var q = uow.Posts.Query()
            .Include(p => p.BlogProfile).ThenInclude(bp => bp.User)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .Where(p => p.Status == PostStatus.Published)
            .Where(p => p.PostTags.Any(pt => pt.TagId == tag.Id));

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
            Items = items.Select(p => new PostSummaryDto
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Excerpt = p.Excerpt,
                CoverImageUrl = p.CoverImageUrl,
                Status = p.Status.ToString(),
                PublishedAt = p.PublishedAt,
                ReadTimeMinutes = p.ReadTimeMinutes,
                ViewCount = p.ViewCount,
                LikesCount = p.LikesCount,
                CommentsCount = p.CommentsCount,
                Tags = p.PostTags.Select(pt => new PostTagDto { Id = pt.Tag.Id, Name = pt.Tag.Name, Slug = pt.Tag.Slug }).ToList(),
                Author = new AuthorDto
                {
                    UserId = p.BlogProfile.UserId,
                    Username = p.BlogProfile.User.UserName ?? string.Empty,
                    DisplayName = p.BlogProfile.User.DisplayName,
                    AvatarUrl = p.BlogProfile.User.AvatarUrl
                },
                CreatedAt = p.CreatedAt
            }).ToList(),
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
}
