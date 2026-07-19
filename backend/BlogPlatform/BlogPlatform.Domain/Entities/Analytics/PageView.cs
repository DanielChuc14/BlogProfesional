using BlogPlatform.Domain.Enums;

namespace BlogPlatform.Domain.Entities.Analytics;

public class PageView
{
    public long Id { get; set; }
    public Guid PostId { get; set; }
    public Guid BlogProfileId { get; set; }
    /// <summary>SHA-256(Salt:Date:IP:UserAgent) — IP is never stored directly.</summary>
    public string VisitorHash { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public string? Referrer { get; set; }
    public DeviceType DeviceType { get; set; } = DeviceType.Unknown;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
