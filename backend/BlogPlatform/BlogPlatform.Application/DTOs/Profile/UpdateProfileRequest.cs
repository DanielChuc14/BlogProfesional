namespace BlogPlatform.Application.DTOs.Profile;

public class UpdateProfileRequest
{
    public string? DisplayName { get; init; }
    public string? Bio { get; init; }
    public string? About { get; init; }
    public string? LogoUrl { get; init; }
    public string? BannerUrl { get; init; }
    public string? Tagline { get; init; }
    public IReadOnlyList<SocialLinkRequest> SocialLinks { get; init; } = [];
}

public class SocialLinkRequest
{
    public string Platform { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}
