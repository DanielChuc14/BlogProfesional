using BlogPlatform.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BlogPlatform.Infrastructure.Services;

public class SendGridEmailService(IConfiguration configuration, ILogger<SendGridEmailService> logger) : IEmailService
{
    private readonly string _apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")
        ?? configuration["Email:SendGridApiKey"]
        ?? throw new InvalidOperationException("SendGrid API key is not configured.");

    private readonly string _fromEmail = configuration["Email:FromAddress"] ?? "noreply@blogplatform.com";
    private readonly string _fromName = configuration["Email:FromName"] ?? "BlogPlatform";

    private async Task SendAsync(string to, string subject, string htmlContent, CancellationToken ct)
    {
        var client = new SendGridClient(_apiKey);
        var msg = MailHelper.CreateSingleEmail(
            new EmailAddress(_fromEmail, _fromName),
            new EmailAddress(to),
            subject,
            plainTextContent: null,
            htmlContent: htmlContent);

        var response = await client.SendEmailAsync(msg, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Body.ReadAsStringAsync(ct);
            logger.LogError("SendGrid error sending to {To}: {Status} - {Body}", to, response.StatusCode, body);
            throw new Exception($"SendGrid returned {response.StatusCode}");
        }
    }

    public Task SendEmailConfirmationAsync(string to, string token, CancellationToken ct = default)
        => SendAsync(to, "Confirm your email", $"<p>Your confirmation token: <strong>{token}</strong></p>", ct);

    public Task SendPasswordResetAsync(string to, string token, CancellationToken ct = default)
        => SendAsync(to, "Reset your password", $"<p>Your password reset token: <strong>{token}</strong></p>", ct);

    public Task SendWelcomeAsync(string to, string displayName, CancellationToken ct = default)
        => SendAsync(to, "Welcome to BlogPlatform", $"<p>Welcome, <strong>{displayName}</strong>! Your account is ready.</p>", ct);

    public Task SendNewFollowerAsync(string to, string followerName, CancellationToken ct = default)
        => SendAsync(to, "New follower", $"<p><strong>{followerName}</strong> started following you.</p>", ct);

    public Task SendNewCommentAsync(string to, string postTitle, string commenterName, CancellationToken ct = default)
        => SendAsync(to, $"New comment on \"{postTitle}\"",
            $"<p><strong>{commenterName}</strong> commented on your post <em>{postTitle}</em>.</p>", ct);

    public Task SendNewsletterAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
        => SendAsync(to, subject, htmlBody, ct);
}
