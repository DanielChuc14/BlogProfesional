namespace BlogPlatform.Application.DTOs.Community;

public class NewsletterSendDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int EstimatedRecipients { get; set; }
    public int ActualRecipients { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
