using BlogPlatform.Application.DTOs.Analytics;
using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Enums;

namespace BlogPlatform.Application.Interfaces;

public interface IAnalyticsService
{
    Task TrackPageViewAsync(Guid postId, Guid authorUserId, string ipAddress, string userAgent, string? referrer, CancellationToken ct = default);
    Task<ResultModel<BloggerDashboardDto>> GetBloggerDashboardAsync(Guid userId, CancellationToken ct = default);
    Task<ResultModel<AdminDashboardDto>> GetAdminDashboardAsync(CancellationToken ct = default);
    Task<ResultModel<PostAnalyticsDto>> GetPostAnalyticsAsync(Guid postId, Guid requestingUserId, CancellationToken ct = default);
}
