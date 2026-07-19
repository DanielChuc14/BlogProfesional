using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Entities.Content;
using BlogPlatform.Domain.Enums;

namespace BlogPlatform.Domain.Entities.Community;

public class NewsletterSend : BaseEntity
{
    public Guid BlogProfileId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public NewsletterSendStatus Status { get; set; } = NewsletterSendStatus.Pending;
    public int EstimatedRecipients { get; set; }
    public int ActualRecipients { get; set; }
    public DateTime? SentAt { get; set; }

    // Navigation
    public BlogProfile BlogProfile { get; set; } = null!;
}
