using BlogPlatform.Domain.Entities.Content;
using Microsoft.AspNetCore.Identity;

namespace BlogPlatform.Domain.Entities.Auth;

public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Profile fields
    public string? Country { get; set; }
    public string? Website { get; set; }
    public string? TwitterHandle { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? ProfileHeaderColor { get; set; }

    // Preferences
    public string PreferredLanguage { get; set; } = "en";
    public bool ReceiveEmailNotifications { get; set; } = true;
    public string ProfileVisibility { get; set; } = "public";

    // Suspension
    public DateTime? SuspendedUntil { get; set; }

    // Storage
    public long StorageUsedBytes { get; set; } = 0;
    public long StorageLimitBytes { get; set; } = 1_073_741_824; // 1 GB
    public bool AllowAdultContent { get; set; } = false;

    // Navigation
    public BlogProfile? Profile { get; set; }
}
