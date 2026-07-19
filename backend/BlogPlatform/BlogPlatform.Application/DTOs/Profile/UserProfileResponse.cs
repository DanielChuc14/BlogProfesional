namespace BlogPlatform.Application.DTOs.Profile;

public class UserProfileResponse
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? Bio { get; init; }
    public string? AvatarUrl { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string? About { get; init; }
    public string? LogoUrl { get; init; }
    public string? BannerUrl { get; init; }
    public string? Tagline { get; init; }
    public BlogThemeDto? Theme { get; init; }
    public IReadOnlyList<SocialLinkDto> SocialLinks { get; init; } = [];
    public IReadOnlyList<BlogNoticeDto> ActiveNotices { get; init; } = [];
    public IReadOnlyList<QuickLinkDto> QuickLinks { get; init; } = [];
    public int FollowersCount { get; init; }
    public int FollowingCount { get; init; }
    public int PostsCount { get; init; }
    public bool IsFollowing { get; init; }
    public bool IsBlocked { get; init; }
    public DateTime CreatedAt { get; init; }
}
