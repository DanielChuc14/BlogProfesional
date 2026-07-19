using BlogPlatform.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class PostMediaConfiguration : IEntityTypeConfiguration<PostMedia>
{
    public void Configure(EntityTypeBuilder<PostMedia> builder)
    {
        builder.ToTable("post_media");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Url)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.Caption).HasMaxLength(300);

        builder.HasIndex(x => x.PostId);
    }
}
