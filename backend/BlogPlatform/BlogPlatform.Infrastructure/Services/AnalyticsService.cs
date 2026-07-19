using System.Security.Cryptography;
using System.Text;
using BlogPlatform.Application.DTOs.Analytics;
using BlogPlatform.Application.Interfaces;
using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Entities.Analytics;
using BlogPlatform.Domain.Enums;
using BlogPlatform.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Infrastructure.Services;

public class AnalyticsService(
    IUnitOfWork uow,
    IConfiguration configuration,
    ILogger<AnalyticsService> logger) : IAnalyticsService
{
    public async Task TrackPageViewAsync(
        Guid postId, Guid authorUserId,
        string ipAddress, string userAgent, string? referrer,
        CancellationToken ct = default)
    {
        try
        {
            var profile = await uow.BlogProfiles.Query()
                .FirstOrDefaultAsync(bp => bp.UserId == authorUserId, ct);

            if (profile is null) return;

            var blogProfileId = profile.Id;

            var salt = configuration["Analytics:DailySaltPrefix"] ?? "blogplatform";
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var raw = $"{salt}:{today}:{ipAddress}:{userAgent}";
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();

            var deviceType = DetectDeviceType(userAgent);

            await uow.PageViews.AddAsync(new PageView
            {
                PostId = postId,
                BlogProfileId = blogProfileId,
                VisitorHash = hash,
                UserAgent = userAgent.Length > 512 ? userAgent[..512] : userAgent,
                Referrer = referrer is not null && referrer.Length > 512 ? referrer[..512] : referrer,
                DeviceType = deviceType
            }, ct);

            await uow.SaveChangesAsync(ct);

            // Upsert daily stat
            await UpsertDailyStatAsync(blogProfileId, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to track page view for post {PostId}", postId);
        }
    }

    public async Task<ResultModel<BloggerDashboardDto>> GetBloggerDashboardAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await uow.BlogProfiles.Query()
            .FirstOrDefaultAsync(bp => bp.UserId == userId, ct);

        if (profile is null)
            return ResultModel<BloggerDashboardDto>.NotFound("Blog profile not found.");

        var since = DateTime.UtcNow.AddDays(-30);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var since30 = DateOnly.FromDateTime(since);

        var posts = await uow.Posts.Query()
            .Where(p => p.BlogProfileId == profile.Id)
            .ToListAsync(ct);

        var dailyStats = await uow.DailyStats.Query()
            .Where(ds => ds.BlogProfileId == profile.Id && ds.Date >= since30)
            .OrderBy(ds => ds.Date)
            .ToListAsync(ct);

        var followersCount = await uow.Follows.Query()
            .CountAsync(f => f.BlogProfileId == profile.Id, ct);

        var topPosts = posts
            .OrderByDescending(p => p.ViewCount)
            .Take(5)
            .Select(p => new TopPostDto
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                ViewCount = p.ViewCount,
                LikesCount = p.LikesCount,
                CommentsCount = p.CommentsCount,
                PublishedAt = p.PublishedAt
            }).ToList();

        var dto = new BloggerDashboardDto
        {
            TotalPosts = posts.Count,
            TotalViews = posts.Sum(p => p.ViewCount),
            TotalLikes = posts.Sum(p => p.LikesCount),
            TotalComments = posts.Sum(p => p.CommentsCount),
            TotalFollowers = followersCount,
            ViewsLast30Days = dailyStats.Sum(ds => ds.ViewCount),
            LikesLast30Days = dailyStats.Sum(ds => ds.LikesCount),
            CommentsLast30Days = dailyStats.Sum(ds => ds.CommentsCount),
            DailyStats = dailyStats.Select(ds => new DailyStatDto
            {
                Date = ds.Date,
                ViewCount = ds.ViewCount,
                UniqueVisitors = ds.UniqueVisitors,
                NewFollowers = ds.NewFollowers,
                LikesCount = ds.LikesCount,
                CommentsCount = ds.CommentsCount
            }).ToList(),
            TopPosts = topPosts
        };

        return ResultModel<BloggerDashboardDto>.Ok(dto);
    }

    public async Task<ResultModel<AdminDashboardDto>> GetAdminDashboardAsync(CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddDays(-30);
        var since30 = DateOnly.FromDateTime(since);

        var totalUsers = await uow.BlogProfiles.Query().CountAsync(ct);
        var totalPosts = await uow.Posts.Query().CountAsync(ct);
        var totalComments = await uow.Comments.Query().CountAsync(ct);
        var totalPageViews = await uow.PageViews.Query().CountAsync(ct);

        var newUsers = await uow.BlogProfiles.Query()
            .CountAsync(bp => bp.CreatedAt >= since, ct);

        var newPosts = await uow.Posts.Query()
            .CountAsync(p => p.CreatedAt >= since, ct);

        var pageViewsLast30 = await uow.PageViews.Query()
            .CountAsync(pv => pv.CreatedAt >= since, ct);

        var activeBloggers = await uow.Posts.Query()
            .Where(p => p.CreatedAt >= since)
            .Select(p => p.BlogProfileId)
            .Distinct()
            .CountAsync(ct);

        var dto = new AdminDashboardDto
        {
            TotalUsers = totalUsers,
            TotalPosts = totalPosts,
            TotalComments = totalComments,
            TotalPageViews = totalPageViews,
            NewUsersLast30Days = newUsers,
            NewPostsLast30Days = newPosts,
            PageViewsLast30Days = pageViewsLast30,
            ActiveBloggers = activeBloggers
        };

        return ResultModel<AdminDashboardDto>.Ok(dto);
    }

    public async Task<ResultModel<PostAnalyticsDto>> GetPostAnalyticsAsync(Guid postId, Guid requestingUserId, CancellationToken ct = default)
    {
        var post = await uow.Posts.Query()
            .Include(p => p.BlogProfile)
            .FirstOrDefaultAsync(p => p.Id == postId, ct);

        if (post is null)
            return ResultModel<PostAnalyticsDto>.NotFound("Post not found.");

        if (post.BlogProfile.UserId != requestingUserId)
            return ResultModel<PostAnalyticsDto>.Unauthorized("Not authorized to view analytics for this post.");

        var pageViews = await uow.PageViews.Query()
            .Where(pv => pv.PostId == postId)
            .ToListAsync(ct);

        var viewsByDay = pageViews
            .GroupBy(pv => pv.CreatedAt.ToString("yyyy-MM-dd"))
            .ToDictionary(g => g.Key, g => g.Count());

        var viewsByDevice = pageViews
            .GroupBy(pv => pv.DeviceType.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var topReferrers = pageViews
            .Where(pv => !string.IsNullOrEmpty(pv.Referrer))
            .GroupBy(pv => pv.Referrer!)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToDictionary(g => g.Key, g => g.Count());

        var dto = new PostAnalyticsDto
        {
            PostId = post.Id,
            Title = post.Title,
            TotalViews = post.ViewCount,
            UniqueVisitors = pageViews.Select(pv => pv.VisitorHash).Distinct().Count(),
            LikesCount = post.LikesCount,
            CommentsCount = post.CommentsCount,
            ViewsByDay = viewsByDay,
            ViewsByDevice = viewsByDevice,
            TopReferrers = topReferrers
        };

        return ResultModel<PostAnalyticsDto>.Ok(dto);
    }

    private async Task UpsertDailyStatAsync(Guid blogProfileId, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var stat = await uow.DailyStats.Query()
            .FirstOrDefaultAsync(ds => ds.BlogProfileId == blogProfileId && ds.Date == today, ct);

        if (stat is null)
        {
            stat = new DailyStat
            {
                BlogProfileId = blogProfileId,
                Date = today,
                ViewCount = 1
            };
            await uow.DailyStats.AddAsync(stat, ct);
        }
        else
        {
            stat.ViewCount++;
            uow.DailyStats.Update(stat);
        }

        await uow.SaveChangesAsync(ct);
    }

    private static DeviceType DetectDeviceType(string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent)) return DeviceType.Unknown;
        var ua = userAgent.ToLowerInvariant();
        if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone")) return DeviceType.Mobile;
        if (ua.Contains("tablet") || ua.Contains("ipad")) return DeviceType.Tablet;
        if (ua.Contains("mozilla") || ua.Contains("chrome") || ua.Contains("safari")) return DeviceType.Desktop;
        return DeviceType.Unknown;
    }
}
