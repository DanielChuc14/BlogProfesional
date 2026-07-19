using BlogPlatform.Domain.Entities.Auth;

namespace BlogPlatform.Domain.Entities.Content;

public class BlogProfile : Common.BaseEntity
{
    public Guid UserId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? About { get; set; }
    public bool IsMonetizationEnabled { get; set; } = false;
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? Tagline { get; set; }

    // Navigation
    public ApplicationUser User { get; set; } = null!;
    public BlogTheme? Theme { get; set; }
    public ICollection<SocialLink> SocialLinks { get; set; } = [];
    public ICollection<Post> Posts { get; set; } = [];
    public ICollection<Follow> Followers { get; set; } = [];
    public ICollection<BlogNotice> BlogNotices { get; set; } = [];
    public ICollection<QuickLink> QuickLinks { get; set; } = [];
    public ICollection<BlogList> BlogLists { get; set; } = [];
}
