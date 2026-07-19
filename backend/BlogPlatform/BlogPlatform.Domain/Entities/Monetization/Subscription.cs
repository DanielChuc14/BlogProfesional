using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Enums;

namespace BlogPlatform.Domain.Entities.Monetization;

public class Subscription : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid BlogProfileId { get; set; }
    public Guid BlogPlanId { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Incomplete;
    public string? StripeSubscriptionId { get; set; }
    public DateTime? CurrentPeriodStart { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public DateTime? CanceledAt { get; set; }
}
