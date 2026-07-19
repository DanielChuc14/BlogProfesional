using BlogPlatform.Domain.Entities.Auth;

namespace BlogPlatform.Domain.Entities.Security;

public class UserBlock
{
    public Guid BlockerId { get; set; }
    public Guid BlockedId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ApplicationUser Blocker { get; set; } = null!;
    public ApplicationUser Blocked { get; set; } = null!;
}
