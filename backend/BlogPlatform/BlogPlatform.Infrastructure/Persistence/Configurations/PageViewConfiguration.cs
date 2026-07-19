using BlogPlatform.Domain.Entities.Analytics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class PageViewConfiguration : IEntityTypeConfiguration<PageView>
{
    public void Configure(EntityTypeBuilder<PageView> builder)
    {
        // Table is created as partitioned via SQL raw in migration
        builder.ToTable("page_views", tb => tb.ExcludeFromMigrations());

        builder.HasKey(pv => pv.Id);
        builder.Property(pv => pv.Id).ValueGeneratedOnAdd();

        builder.Property(pv => pv.VisitorHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(pv => pv.UserAgent)
            .HasMaxLength(512);

        builder.Property(pv => pv.Referrer)
            .HasMaxLength(512);

        builder.Property(pv => pv.DeviceType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(pv => pv.PostId);
        builder.HasIndex(pv => pv.BlogProfileId);
        builder.HasIndex(pv => new { pv.BlogProfileId, pv.CreatedAt });
    }
}
