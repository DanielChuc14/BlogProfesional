using BlogPlatform.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class PostLikeConfiguration : IEntityTypeConfiguration<PostLike>
{
    public void Configure(EntityTypeBuilder<PostLike> builder)
    {
        builder.ToTable("post_likes");
        builder.HasKey(pl => new { pl.PostId, pl.UserId });

        builder.HasOne(pl => pl.Post)
            .WithMany(p => p.Likes)
            .HasForeignKey(pl => pl.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pl => pl.User)
            .WithMany()
            .HasForeignKey(pl => pl.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
