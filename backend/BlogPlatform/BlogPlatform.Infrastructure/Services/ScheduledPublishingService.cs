using BlogPlatform.Domain.Enums;
using BlogPlatform.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Infrastructure.Services;

public class ScheduledPublishingService(
    IServiceScopeFactory scopeFactory,
    ILogger<ScheduledPublishingService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Scheduled publishing service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await PublishScheduledPostsAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task PublishScheduledPostsAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var now = DateTime.UtcNow;
        var due = await uow.Posts.Query()
            .Where(p => p.Status == PostStatus.Scheduled && p.ScheduledAt <= now)
            .ToListAsync(ct);

        if (due.Count == 0) return;

        foreach (var post in due)
        {
            post.Status = PostStatus.Published;
            post.PublishedAt = now;
            post.ScheduledAt = null;
            uow.Posts.Update(post);
        }

        await uow.SaveChangesAsync(ct);
        logger.LogInformation("Published {Count} scheduled post(s).", due.Count);
    }
}
