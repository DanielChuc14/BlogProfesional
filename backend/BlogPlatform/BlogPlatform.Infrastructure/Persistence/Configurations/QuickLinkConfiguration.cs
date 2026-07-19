using BlogPlatform.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class QuickLinkConfiguration : IEntityTypeConfiguration<QuickLink>
{
    public void Configure(EntityTypeBuilder<QuickLink> builder)
    {
        builder.ToTable("quick_links");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Url).HasMaxLength(512).IsRequired();
        builder.Property(x => x.Icon).HasMaxLength(50);
        builder.Property(x => x.Order).HasDefaultValue(0);
        builder.HasIndex(x => x.BlogProfileId);

        builder.HasOne(x => x.BlogProfile)
            .WithMany(p => p.QuickLinks)
            .HasForeignKey(x => x.BlogProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
