using BlogPlatform.Domain.Entities.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class PlatformSettingConfiguration : IEntityTypeConfiguration<PlatformSetting>
{
    public void Configure(EntityTypeBuilder<PlatformSetting> builder)
    {
        builder.ToTable("platform_settings");
        builder.HasKey(p => p.Key);
        builder.Property(p => p.Key).HasMaxLength(200);
        builder.Property(p => p.Value).HasMaxLength(4000);
    }
}
