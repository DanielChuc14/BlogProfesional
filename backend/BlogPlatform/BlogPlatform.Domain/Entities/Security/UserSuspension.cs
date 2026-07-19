using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Entities.Auth;

namespace BlogPlatform.Domain.Entities.Security;

public class UserSuspension : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid SuspendedByAdminId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? LiftedByAdminId { get; set; }
    public DateTime? LiftedAt { get; set; }

    // Navigation
    public ApplicationUser User { get; set; } = null!;
    public ApplicationUser SuspendedByAdmin { get; set; } = null!;
    public ApplicationUser? LiftedByAdmin { get; set; }
}
