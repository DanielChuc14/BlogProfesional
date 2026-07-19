namespace BlogPlatform.Application.DTOs.Security;

public class BlockedUserDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime BlockedAt { get; set; }
}
