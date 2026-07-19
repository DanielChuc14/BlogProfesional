namespace BlogPlatform.Domain.Entities.Content;

public class PostSeo : Common.BaseEntity
{
    public Guid PostId { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? OgImageUrl { get; set; }
    public string? CanonicalUrl { get; set; }

    // Navigation
    public Post Post { get; set; } = null!;
}
