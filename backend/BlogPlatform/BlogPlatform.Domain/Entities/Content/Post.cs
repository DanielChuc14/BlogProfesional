using BlogPlatform.Domain.Enums;

namespace BlogPlatform.Domain.Entities.Content;

public class Post : Common.BaseEntity
{
    public Guid BlogProfileId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string? CoverImageUrl { get; set; }
    public PostStatus Status { get; set; } = PostStatus.Draft;
    public DateTime? PublishedAt { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public int ReadTimeMinutes { get; set; }
    public int ViewCount { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public bool IsAdultContent { get; set; } = false;
    public bool IsFeatured { get; set; } = false;
    public int FeaturedOrder { get; set; } = 0;

    // Navigation
    public BlogProfile BlogProfile { get; set; } = null!;
    public PostSeo? Seo { get; set; }
    public ICollection<PostMedia> Media { get; set; } = [];
    public ICollection<PostTag> PostTags { get; set; } = [];
    public ICollection<PostLike> Likes { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
}
