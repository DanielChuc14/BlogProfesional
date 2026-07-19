namespace BlogPlatform.Application.DTOs.Profile;

public class SocialLinkDto
{
    public Guid Id { get; init; }
    public string Platform { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}
