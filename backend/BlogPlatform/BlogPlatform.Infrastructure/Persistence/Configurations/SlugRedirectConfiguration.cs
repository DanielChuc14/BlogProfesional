using BlogPlatform.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class SlugRedirectConfiguration : IEntityTypeConfiguration<SlugRedirect>
{
    public void Configure(EntityTypeBuilder<SlugRedirect> builder)
    {
        builder.ToTable("slug_redirects");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OldSlug)
            .IsRequired()
            .HasMaxLength(120);

        builder.HasIndex(x => x.OldSlug).IsUnique();

        builder.HasOne(x => x.Post)
            .WithMany()
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
