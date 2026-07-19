using BlogPlatform.Domain.Common;

namespace BlogPlatform.Domain.Entities.Content;

public class QuickLink : BaseEntity
{
    public Guid BlogProfileId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int Order { get; set; } = 0;

    // Navigation
    public BlogProfile BlogProfile { get; set; } = null!;
}
