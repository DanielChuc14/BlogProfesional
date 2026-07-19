using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Enums;

namespace BlogPlatform.Domain.Entities.Monetization;

public class StripeAccount : BaseEntity
{
    public Guid BlogProfileId { get; set; }
    public string StripeAccountId { get; set; } = string.Empty;
    public StripeAccountStatus Status { get; set; } = StripeAccountStatus.Pending;
    public bool DetailsSubmitted { get; set; }
    public bool ChargesEnabled { get; set; }
    public bool PayoutsEnabled { get; set; }
    public string? Country { get; set; }
    public string? Currency { get; set; }
}
