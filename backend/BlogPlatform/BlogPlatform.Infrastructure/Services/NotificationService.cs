using System.Text;
using BlogPlatform.Application.DTOs.Common;
using BlogPlatform.Application.DTOs.Community;
using BlogPlatform.Application.Interfaces;
using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Entities.Content;
using BlogPlatform.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Infrastructure.Services;

public class NotificationService(IUnitOfWork uow) : INotificationService
{
    public async Task<ResultModel<CursorPagedResult<NotificationDto>>> GetAsync(
        Guid userId, string? cursor, int pageSize, CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);

        var q = uow.Notifications.Query()
            .Include(n => n.Actor)
            .Include(n => n.Post)
            .Include(n => n.Comment)
            .Where(n => n.RecipientId == userId);

        if (!string.IsNullOrEmpty(cursor))
        {
            var (cursorDate, cursorId) = DecodeCursor(cursor);
            q = q.Where(n => n.CreatedAt < cursorDate ||
                              (n.CreatedAt == cursorDate && n.Id.CompareTo(cursorId) < 0));
        }

        var items = await q
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.Id)
            .Take(pageSize + 1)
            .ToListAsync(ct);

        var hasMore = items.Count > pageSize;
        if (hasMore) items.RemoveAt(items.Count - 1);

        string? nextCursor = null;
        if (hasMore && items.Count > 0)
        {
            var last = items[^1];
            nextCursor = EncodeCursor(last.CreatedAt, last.Id);
        }

        return ResultModel<CursorPagedResult<NotificationDto>>.Ok(new CursorPagedResult<NotificationDto>
        {
            Items = items.Select(MapToDto).ToList(),
            NextCursor = nextCursor,
            HasMore = hasMore
        });
    }

    public async Task<ResultModel<int>> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        var count = await uow.Notifications.Query()
            .CountAsync(n => n.RecipientId == userId && !n.IsRead, ct);
        return ResultModel<int>.Ok(count);
    }

    public async Task<ResultModel> MarkReadAsync(Guid userId, Guid notificationId, CancellationToken ct = default)
    {
        var notification = await uow.Notifications.GetByIdAsync(notificationId, ct);

        if (notification is null || notification.RecipientId != userId)
            return ResultModel.NotFound("Notification not found.");

        notification.IsRead = true;
        uow.Notifications.Update(notification);
        await uow.SaveChangesAsync(ct);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel> MarkAllReadAsync(Guid userId, CancellationToken ct = default)
    {
        var unread = await uow.Notifications.Query()
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var n in unread)
        {
            n.IsRead = true;
            uow.Notifications.Update(n);
        }

        await uow.SaveChangesAsync(ct);
        return ResultModel.NoContent();
    }

    private static NotificationDto MapToDto(Notification n) => new()
    {
        Id = n.Id,
        Type = n.Type.ToString(),
        ActorId = n.ActorId,
        ActorUsername = n.Actor.UserName ?? string.Empty,
        ActorDisplayName = n.Actor.DisplayName,
        ActorAvatarUrl = n.Actor.AvatarUrl,
        PostId = n.PostId,
        PostSlug = n.Post?.Slug,
        PostTitle = n.Post?.Title,
        CommentId = n.CommentId,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt
    };

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
