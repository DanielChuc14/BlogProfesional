namespace BlogPlatform.Application.DTOs.Community;

public class SendNewsletterResponse
{
    public Guid SendId { get; set; }
    public int EstimatedRecipients { get; set; }
    public DateTime CanSendAfter { get; set; }
}
