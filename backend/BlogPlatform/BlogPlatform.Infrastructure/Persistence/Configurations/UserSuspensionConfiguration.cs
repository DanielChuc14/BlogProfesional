using BlogPlatform.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class UserSuspensionConfiguration : IEntityTypeConfiguration<UserSuspension>
{
    public void Configure(EntityTypeBuilder<UserSuspension> builder)
    {
        builder.ToTable("user_suspensions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Reason).HasMaxLength(500);

        builder.HasIndex(s => new { s.UserId, s.IsActive });

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.SuspendedByAdmin)
            .WithMany()
            .HasForeignKey(s => s.SuspendedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.LiftedByAdmin)
            .WithMany()
            .HasForeignKey(s => s.LiftedByAdminId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
