using BlogPlatform.Domain.Enums;

namespace BlogPlatform.Domain.Entities.Content;

public class PostMedia : Common.BaseEntity
{
    public Guid PostId { get; set; }
    public string Url { get; set; } = string.Empty;
    public MediaType Type { get; set; }
    public string? Caption { get; set; }
    public int SortOrder { get; set; }

    // Navigation
    public Post Post { get; set; } = null!;
}
