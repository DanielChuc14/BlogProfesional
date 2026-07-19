namespace BlogPlatform.Application.DTOs.Community;

public class FollowerDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string ProfileSlug { get; set; } = string.Empty;
}
