using BlogPlatform.Domain.Entities.Monetization;
using BlogPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class StripeAccountConfiguration : IEntityTypeConfiguration<StripeAccount>
{
    public void Configure(EntityTypeBuilder<StripeAccount> builder)
    {
        builder.ToTable("stripe_accounts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StripeAccountId).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Country).HasMaxLength(2);
        builder.Property(x => x.Currency).HasMaxLength(3);

        builder.HasIndex(x => x.BlogProfileId).IsUnique();
        builder.HasIndex(x => x.StripeAccountId).IsUnique();
    }
}
