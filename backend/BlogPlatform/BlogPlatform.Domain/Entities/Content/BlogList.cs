using BlogPlatform.Domain.Common;

namespace BlogPlatform.Domain.Entities.Content;

public class BlogList : BaseEntity
{
    public Guid BlogProfileId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = true;
    public string? CoverImageUrl { get; set; }
    public int Order { get; set; } = 0;

    // Navigation
    public BlogProfile BlogProfile { get; set; } = null!;
    public ICollection<BlogListItem> Items { get; set; } = [];
}
