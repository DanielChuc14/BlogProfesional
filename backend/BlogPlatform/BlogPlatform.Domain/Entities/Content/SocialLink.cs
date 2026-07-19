namespace BlogPlatform.Domain.Entities.Content;

public class SocialLink : Common.BaseEntity
{
    public Guid BlogProfileId { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    // Navigation
    public BlogProfile BlogProfile { get; set; } = null!;
}
