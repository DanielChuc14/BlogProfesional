namespace BlogPlatform.Domain.Entities.Content;

public class Tag : Common.BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int PostCount { get; set; }

    // Navigation
    public ICollection<PostTag> PostTags { get; set; } = [];
}
