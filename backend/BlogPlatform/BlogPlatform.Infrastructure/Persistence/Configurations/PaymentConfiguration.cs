using BlogPlatform.Domain.Entities.Monetization;
using BlogPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(3);
        builder.Property(x => x.StripePaymentIntentId).HasMaxLength(100);
        builder.Property(x => x.StripeInvoiceId).HasMaxLength(100);

        // ON DELETE RESTRICT — datos financieros no se eliminan en cascada
        builder.HasOne<Subscription>()
            .WithMany()
            .HasForeignKey(x => x.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.SubscriptionId);
        builder.HasIndex(x => x.StripePaymentIntentId);
    }
}
