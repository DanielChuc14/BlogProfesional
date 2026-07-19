using BlogPlatform.Domain.Entities.Content;
using BlogPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasMaxLength(350)
            .IsRequired();

        builder.Property(x => x.Excerpt)
            .HasMaxLength(500);

        builder.Property(x => x.CoverImageUrl)
            .HasMaxLength(512);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.ViewCount).HasDefaultValue(0);
        builder.Property(x => x.LikesCount).HasDefaultValue(0);
        builder.Property(x => x.CommentsCount).HasDefaultValue(0);
        builder.Property(x => x.IsAdultContent).HasDefaultValue(false);
        builder.Property(x => x.IsFeatured).HasDefaultValue(false);
        builder.Property(x => x.FeaturedOrder).HasDefaultValue(0);

        // Shadow property for full-text search
        builder.Property<NpgsqlTsVector>("SearchVector")
            .HasColumnType("tsvector")
            .IsRequired(false);

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.BlogProfileId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.PublishedAt);
        builder.HasIndex("SearchVector").HasMethod("GIN");

        builder.HasOne(x => x.BlogProfile)
            .WithMany(p => p.Posts)
            .HasForeignKey(x => x.BlogProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Seo)
            .WithOne(s => s.Post)
            .HasForeignKey<PostSeo>(s => s.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Media)
            .WithOne(m => m.Post)
            .HasForeignKey(m => m.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.PostTags)
            .WithOne(pt => pt.Post)
            .HasForeignKey(pt => pt.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
