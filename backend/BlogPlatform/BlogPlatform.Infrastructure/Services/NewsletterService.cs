using BlogPlatform.Application.DTOs.Community;
using BlogPlatform.Application.Interfaces;
using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Entities.Community;
using BlogPlatform.Domain.Enums;
using BlogPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Infrastructure.Services;

public class NewsletterService(
    AppDbContext db,
    IEmailService emailService,
    ILogger<NewsletterService> logger) : INewsletterService
{
    private const int CooldownHours = 24;
    private const int BatchSize = 100;

    public async Task<ResultModel<SendNewsletterResponse>> InitiateAsync(
        Guid authorId,
        SendNewsletterRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Subject))
            return ResultModel<SendNewsletterResponse>.BadRequest("Subject is required.");

        if (string.IsNullOrWhiteSpace(request.HtmlBody))
            return ResultModel<SendNewsletterResponse>.BadRequest("HtmlBody is required.");

        var profile = await db.BlogProfiles.FirstOrDefaultAsync(p => p.UserId == authorId, ct);
        if (profile is null)
            return ResultModel<SendNewsletterResponse>.NotFound("Blog profile not found.");

        var cooldownFrom = DateTime.UtcNow.AddHours(-CooldownHours);
        var lastSent = await db.Set<NewsletterSend>()
            .Where(n => n.BlogProfileId == profile.Id && n.Status == NewsletterSendStatus.Sent && n.SentAt > cooldownFrom)
            .OrderByDescending(n => n.SentAt)
            .FirstOrDefaultAsync(ct);

        if (lastSent is not null)
        {
            var canSendAfter = lastSent.SentAt!.Value.AddHours(CooldownHours);
            return ResultModel<SendNewsletterResponse>.BadRequest(
                $"Newsletter already sent recently. You can send again after {canSendAfter:u}.");
        }

        var estimatedRecipients = await db.Follows
            .CountAsync(f => f.BlogProfileId == profile.Id, ct);

        var send = new NewsletterSend
        {
            BlogProfileId = profile.Id,
            Subject = request.Subject.Trim(),
            HtmlBody = request.HtmlBody,
            Status = NewsletterSendStatus.Pending,
            EstimatedRecipients = estimatedRecipients
        };

        db.Set<NewsletterSend>().Add(send);
        await db.SaveChangesAsync(ct);

        return ResultModel<SendNewsletterResponse>.Created(new SendNewsletterResponse
        {
            SendId = send.Id,
            EstimatedRecipients = estimatedRecipients,
            CanSendAfter = DateTime.UtcNow.AddHours(CooldownHours)
        });
    }

    public async Task<ResultModel<NewsletterSendDto>> ConfirmSendAsync(
        Guid sendId,
        Guid authorId,
        CancellationToken ct = default)
    {
        var profile = await db.BlogProfiles.FirstOrDefaultAsync(p => p.UserId == authorId, ct);
        if (profile is null)
            return ResultModel<NewsletterSendDto>.NotFound("Blog profile not found.");

        var send = await db.Set<NewsletterSend>().FindAsync([sendId], ct);
        if (send is null)
            return ResultModel<NewsletterSendDto>.NotFound("Newsletter send not found.");

        if (send.BlogProfileId != profile.Id)
            return ResultModel<NewsletterSendDto>.Forbidden("Access denied.");

        if (send.Status != NewsletterSendStatus.Pending)
            return ResultModel<NewsletterSendDto>.Conflict($"Newsletter is in '{send.Status}' status and cannot be confirmed.");

        send.Status = NewsletterSendStatus.Sending;
        await db.SaveChangesAsync(ct);

        var emails = await db.Follows
            .Where(f => f.BlogProfileId == profile.Id && f.Follower.Email != null)
            .Select(f => f.Follower.Email!)
            .ToListAsync(ct);

        var sent = 0;
        try
        {
            for (int i = 0; i < emails.Count; i += BatchSize)
            {
                var batch = emails.Skip(i).Take(BatchSize);
                var tasks = batch.Select(email =>
                    emailService.SendNewsletterAsync(email, send.Subject, send.HtmlBody, ct));
                await Task.WhenAll(tasks);
                sent += batch.Count();
            }

            send.Status = NewsletterSendStatus.Sent;
            send.ActualRecipients = sent;
            send.SentAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Newsletter send {SendId} failed after {Sent} emails", sendId, sent);
            send.Status = NewsletterSendStatus.Failed;
            send.ActualRecipients = sent;
        }

        await db.SaveChangesAsync(ct);

        return ResultModel<NewsletterSendDto>.Ok(new NewsletterSendDto
        {
            Id = send.Id,
            Subject = send.Subject,
            Status = send.Status.ToString(),
            EstimatedRecipients = send.EstimatedRecipients,
            ActualRecipients = send.ActualRecipients,
            SentAt = send.SentAt,
            CreatedAt = send.CreatedAt
        });
    }
}
