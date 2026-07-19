using BlogPlatform.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class BlogThemeConfiguration : IEntityTypeConfiguration<BlogTheme>
{
    public void Configure(EntityTypeBuilder<BlogTheme> builder)
    {
        builder.ToTable("blog_themes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Config)
            .HasColumnType("jsonb")
            .IsRequired();
    }
}
