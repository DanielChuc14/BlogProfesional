using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Entities.Auth;
using BlogPlatform.Domain.Enums;

namespace BlogPlatform.Domain.Entities.Security;

public class Report : BaseEntity
{
    public Guid ReporterId { get; set; }
    public ReportTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }
    public ReportReason Reason { get; set; }
    public string? Description { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public Guid? ReviewedByAdminId { get; set; }
    public string? AdminNote { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Navigation
    public ApplicationUser Reporter { get; set; } = null!;
    public ApplicationUser? ReviewedByAdmin { get; set; }
}
