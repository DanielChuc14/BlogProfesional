using BlogPlatform.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class BlogListItemConfiguration : IEntityTypeConfiguration<BlogListItem>
{
    public void Configure(EntityTypeBuilder<BlogListItem> builder)
    {
        builder.ToTable("blog_list_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Order).HasDefaultValue(0);
        builder.HasIndex(x => x.BlogListId);
        builder.HasIndex(x => new { x.BlogListId, x.PostId }).IsUnique();

        builder.HasOne(x => x.Post)
            .WithMany()
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
