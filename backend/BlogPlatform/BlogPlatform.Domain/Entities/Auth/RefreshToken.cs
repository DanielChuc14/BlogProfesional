using BlogPlatform.Domain.Common;

namespace BlogPlatform.Domain.Entities.Auth;

public class RefreshToken : BaseEntity
{
    /// <summary>SHA-256 hash of the actual token value. The raw token is only returned once at creation.</summary>
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public string? RevokedReason { get; set; }
    public string? DeviceInfo { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
}
