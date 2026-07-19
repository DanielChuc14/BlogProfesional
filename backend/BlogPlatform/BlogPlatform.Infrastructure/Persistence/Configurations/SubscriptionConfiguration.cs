using BlogPlatform.Domain.Entities.Monetization;
using BlogPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.StripeSubscriptionId).HasMaxLength(100);

        // ON DELETE RESTRICT — datos financieros no se eliminan en cascada
        builder.HasOne<BlogPlan>()
            .WithMany()
            .HasForeignKey(x => x.BlogPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.BlogProfileId);
        builder.HasIndex(x => x.StripeSubscriptionId);
    }
}
