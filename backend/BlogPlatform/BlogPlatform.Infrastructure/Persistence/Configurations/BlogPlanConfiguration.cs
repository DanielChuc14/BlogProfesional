using BlogPlatform.Domain.Entities.Monetization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class BlogPlanConfiguration : IEntityTypeConfiguration<BlogPlan>
{
    public void Configure(EntityTypeBuilder<BlogPlan> builder)
    {
        builder.ToTable("blog_plans");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(3);
        builder.Property(x => x.StripePriceId).HasMaxLength(100);
        builder.Property(x => x.IsActive).HasDefaultValue(false);

        builder.HasIndex(x => x.BlogProfileId);
    }
}
