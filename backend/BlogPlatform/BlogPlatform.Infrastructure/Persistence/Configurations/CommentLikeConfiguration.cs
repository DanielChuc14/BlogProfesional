using BlogPlatform.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class CommentLikeConfiguration : IEntityTypeConfiguration<CommentLike>
{
    public void Configure(EntityTypeBuilder<CommentLike> builder)
    {
        builder.ToTable("comment_likes");
        builder.HasKey(cl => new { cl.CommentId, cl.UserId });

        builder.HasOne(cl => cl.Comment)
            .WithMany(c => c.CommentLikes)
            .HasForeignKey(cl => cl.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cl => cl.User)
            .WithMany()
            .HasForeignKey(cl => cl.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
