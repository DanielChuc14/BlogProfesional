namespace BlogPlatform.Application.DTOs.Community;

public class SendNewsletterRequest
{
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
}
