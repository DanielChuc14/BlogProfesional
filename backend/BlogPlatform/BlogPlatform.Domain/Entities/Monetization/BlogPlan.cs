using BlogPlatform.Domain.Common;

namespace BlogPlatform.Domain.Entities.Monetization;

public class BlogPlan : BaseEntity
{
    public Guid BlogProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    /// <summary>Price in cents — never store as decimal.</summary>
    public int PriceCents { get; set; }
    public string Currency { get; set; } = "usd";
    public bool IsActive { get; set; }
    public string? StripePriceId { get; set; }
}
