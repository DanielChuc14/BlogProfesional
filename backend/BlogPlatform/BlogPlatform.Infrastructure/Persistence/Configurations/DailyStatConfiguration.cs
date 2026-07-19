using BlogPlatform.Domain.Entities.Analytics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class DailyStatConfiguration : IEntityTypeConfiguration<DailyStat>
{
    public void Configure(EntityTypeBuilder<DailyStat> builder)
    {
        builder.ToTable("daily_stats");
        builder.HasKey(ds => ds.Id);

        builder.Property(ds => ds.ViewCount).HasDefaultValue(0);
        builder.Property(ds => ds.UniqueVisitors).HasDefaultValue(0);
        builder.Property(ds => ds.NewFollowers).HasDefaultValue(0);
        builder.Property(ds => ds.LikesCount).HasDefaultValue(0);
        builder.Property(ds => ds.CommentsCount).HasDefaultValue(0);

        builder.HasOne(ds => ds.BlogProfile)
            .WithMany()
            .HasForeignKey(ds => ds.BlogProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ds => new { ds.BlogProfileId, ds.Date }).IsUnique();
        builder.HasIndex(ds => ds.Date);
    }
}
