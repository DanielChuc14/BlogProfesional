using BlogPlatform.Domain.Common;

namespace BlogPlatform.Domain.Entities.Admin;

public class AuditLog : BaseEntity
{
    public Guid ActorId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Reason { get; set; }
    public string? IpHash { get; set; }
}
