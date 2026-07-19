using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Enums;

namespace BlogPlatform.Domain.Entities.Content;

public class BlogNotice : BaseEntity
{
    public Guid BlogProfileId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public BlogNoticeType Type { get; set; } = BlogNoticeType.Info;
    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }
    public int Priority { get; set; } = 0;

    // Navigation
    public BlogProfile BlogProfile { get; set; } = null!;
}
