using BlogPlatform.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Bio)
            .HasMaxLength(500);

        builder.Property(x => x.AvatarUrl)
            .HasMaxLength(512);

        builder.Property(x => x.Country)
            .HasMaxLength(100);

        builder.Property(x => x.Website)
            .HasMaxLength(512);

        builder.Property(x => x.TwitterHandle)
            .HasMaxLength(100);

        builder.Property(x => x.LinkedInUrl)
            .HasMaxLength(512);

        builder.Property(x => x.GitHubUrl)
            .HasMaxLength(512);

        builder.Property(x => x.ProfileHeaderColor)
            .HasMaxLength(20);
    }
}
