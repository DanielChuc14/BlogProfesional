using BlogPlatform.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class BlogNoticeConfiguration : IEntityTypeConfiguration<BlogNotice>
{
    public void Configure(EntityTypeBuilder<BlogNotice> builder)
    {
        builder.ToTable("blog_notices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Content).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.Priority).HasDefaultValue(0);
        builder.HasIndex(x => x.BlogProfileId);

        builder.HasOne(x => x.BlogProfile)
            .WithMany(p => p.BlogNotices)
            .HasForeignKey(x => x.BlogProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
