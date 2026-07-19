using BlogPlatform.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class BlogProfileConfiguration : IEntityTypeConfiguration<BlogProfile>
{
    public void Configure(EntityTypeBuilder<BlogProfile> builder)
    {
        builder.ToTable("blog_profiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Slug)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.About)
            .HasMaxLength(2000);

        builder.Property(x => x.IsMonetizationEnabled)
            .HasDefaultValue(false);

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.UserId).IsUnique();

        builder.HasOne(x => x.User)
            .WithOne(u => u.Profile)
            .HasForeignKey<BlogProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Theme)
            .WithOne(t => t.BlogProfile)
            .HasForeignKey<BlogTheme>(t => t.BlogProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.SocialLinks)
            .WithOne(s => s.BlogProfile)
            .HasForeignKey(s => s.BlogProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Posts)
            .WithOne(p => p.BlogProfile)
            .HasForeignKey(p => p.BlogProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
