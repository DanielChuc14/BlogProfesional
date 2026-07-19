using BlogPlatform.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class PostSeoConfiguration : IEntityTypeConfiguration<PostSeo>
{
    public void Configure(EntityTypeBuilder<PostSeo> builder)
    {
        builder.ToTable("post_seos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MetaTitle).HasMaxLength(100);
        builder.Property(x => x.MetaDescription).HasMaxLength(300);
        builder.Property(x => x.OgImageUrl).HasMaxLength(512);
        builder.Property(x => x.CanonicalUrl).HasMaxLength(512);

        builder.HasIndex(x => x.PostId).IsUnique();
    }
}
