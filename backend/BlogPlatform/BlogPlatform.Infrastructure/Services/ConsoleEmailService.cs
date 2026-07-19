using BlogPlatform.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Infrastructure.Services;

public class ConsoleEmailService(ILogger<ConsoleEmailService> logger) : IEmailService
{
    public Task SendEmailConfirmationAsync(string to, string token, CancellationToken ct = default)
    {
        logger.LogInformation("[EMAIL] Confirmation to {To} | Token: {Token}", to, token);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string to, string token, CancellationToken ct = default)
    {
        logger.LogInformation("[EMAIL] Password reset to {To} | Token: {Token}", to, token);
        return Task.CompletedTask;
    }

    public Task SendWelcomeAsync(string to, string displayName, CancellationToken ct = default)
    {
        logger.LogInformation("[EMAIL] Welcome to {To} | Name: {DisplayName}", to, displayName);
        return Task.CompletedTask;
    }

    public Task SendNewFollowerAsync(string to, string followerName, CancellationToken ct = default)
    {
        logger.LogInformation("[EMAIL] New follower to {To} | Follower: {FollowerName}", to, followerName);
        return Task.CompletedTask;
    }

    public Task SendNewCommentAsync(string to, string postTitle, string commenterName, CancellationToken ct = default)
    {
        logger.LogInformation("[EMAIL] New comment to {To} | Post: {PostTitle} | By: {CommenterName}", to, postTitle, commenterName);
        return Task.CompletedTask;
    }

    public Task SendNewsletterAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        logger.LogInformation("[EMAIL] Newsletter to {To} | Subject: {Subject}", to, subject);
        return Task.CompletedTask;
    }
}
