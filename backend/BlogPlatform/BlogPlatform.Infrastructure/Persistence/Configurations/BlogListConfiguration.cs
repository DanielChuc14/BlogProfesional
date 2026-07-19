using BlogPlatform.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class BlogListConfiguration : IEntityTypeConfiguration<BlogList>
{
    public void Configure(EntityTypeBuilder<BlogList> builder)
    {
        builder.ToTable("blog_lists");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Slug).HasMaxLength(250).IsRequired();
        builder.Property(x => x.CoverImageUrl).HasMaxLength(512);
        builder.Property(x => x.IsPublic).HasDefaultValue(true);
        builder.Property(x => x.Order).HasDefaultValue(0);
        builder.HasIndex(x => x.BlogProfileId);
        builder.HasIndex(x => new { x.BlogProfileId, x.Slug }).IsUnique();

        builder.HasOne(x => x.BlogProfile)
            .WithMany(p => p.BlogLists)
            .HasForeignKey(x => x.BlogProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Items)
            .WithOne(i => i.BlogList)
            .HasForeignKey(i => i.BlogListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
