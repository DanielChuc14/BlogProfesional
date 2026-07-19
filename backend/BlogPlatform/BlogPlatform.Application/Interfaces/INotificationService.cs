using BlogPlatform.Application.DTOs.Common;
using BlogPlatform.Application.DTOs.Community;
using BlogPlatform.Domain.Common;

namespace BlogPlatform.Application.Interfaces;

public interface INotificationService
{
    Task<ResultModel<CursorPagedResult<NotificationDto>>> GetAsync(Guid userId, string? cursor, int pageSize, CancellationToken ct = default);
    Task<ResultModel<int>> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
    Task<ResultModel> MarkReadAsync(Guid userId, Guid notificationId, CancellationToken ct = default);
    Task<ResultModel> MarkAllReadAsync(Guid userId, CancellationToken ct = default);
}
