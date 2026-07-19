using BlogPlatform.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class SocialLinkConfiguration : IEntityTypeConfiguration<SocialLink>
{
    public void Configure(EntityTypeBuilder<SocialLink> builder)
    {
        builder.ToTable("social_links");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Platform)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Url)
            .HasMaxLength(512)
            .IsRequired();
    }
}
