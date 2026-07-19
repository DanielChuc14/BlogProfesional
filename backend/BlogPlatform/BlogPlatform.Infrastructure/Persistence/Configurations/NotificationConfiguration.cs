using BlogPlatform.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Type)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(n => n.IsRead)
            .HasDefaultValue(false);

        builder.HasOne(n => n.Recipient)
            .WithMany()
            .HasForeignKey(n => n.RecipientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Actor)
            .WithMany()
            .HasForeignKey(n => n.ActorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.Post)
            .WithMany()
            .HasForeignKey(n => n.PostId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(n => n.Comment)
            .WithMany()
            .HasForeignKey(n => n.CommentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(n => n.RecipientId);
        builder.HasIndex(n => new { n.RecipientId, n.IsRead });
    }
}
