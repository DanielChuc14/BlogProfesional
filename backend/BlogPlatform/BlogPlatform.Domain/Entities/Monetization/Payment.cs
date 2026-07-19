using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Enums;

namespace BlogPlatform.Domain.Entities.Monetization;

public class Payment : BaseEntity
{
    public Guid SubscriptionId { get; set; }
    /// <summary>Amount in cents — never store as decimal.</summary>
    public int AmountCents { get; set; }
    public string Currency { get; set; } = "usd";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? StripePaymentIntentId { get; set; }
    public string? StripeInvoiceId { get; set; }
    public DateTime? PaidAt { get; set; }
}
