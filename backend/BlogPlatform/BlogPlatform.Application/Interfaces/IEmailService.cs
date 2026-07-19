namespace BlogPlatform.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string to, string token, CancellationToken ct = default);
    Task SendPasswordResetAsync(string to, string token, CancellationToken ct = default);
    Task SendWelcomeAsync(string to, string displayName, CancellationToken ct = default);
    Task SendNewFollowerAsync(string to, string followerName, CancellationToken ct = default);
    Task SendNewCommentAsync(string to, string postTitle, string commenterName, CancellationToken ct = default);
    Task SendNewsletterAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
}
