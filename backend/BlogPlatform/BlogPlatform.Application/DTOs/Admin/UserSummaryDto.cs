namespace BlogPlatform.Application.DTOs.Admin;

public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
    public string? ProfileSlug { get; set; }
    public DateTime? SuspendedUntil { get; set; }
    public DateTime CreatedAt { get; set; }
}
