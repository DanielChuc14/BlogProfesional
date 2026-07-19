using BlogPlatform.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("reports");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Description).HasMaxLength(500);
        builder.Property(r => r.AdminNote).HasMaxLength(500);

        builder.HasIndex(r => new { r.ReporterId, r.TargetType, r.TargetId }).IsUnique();
        builder.HasIndex(r => r.Status);

        builder.HasOne(r => r.Reporter)
            .WithMany()
            .HasForeignKey(r => r.ReporterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.ReviewedByAdmin)
            .WithMany()
            .HasForeignKey(r => r.ReviewedByAdminId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
