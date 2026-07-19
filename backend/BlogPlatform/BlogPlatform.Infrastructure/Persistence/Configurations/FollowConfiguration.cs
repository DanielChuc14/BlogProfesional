using BlogPlatform.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class FollowConfiguration : IEntityTypeConfiguration<Follow>
{
    public void Configure(EntityTypeBuilder<Follow> builder)
    {
        builder.ToTable("follows");
        builder.HasKey(f => new { f.FollowerId, f.BlogProfileId });

        builder.HasOne(f => f.Follower)
            .WithMany()
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.BlogProfile)
            .WithMany(bp => bp.Followers)
            .HasForeignKey(f => f.BlogProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => f.FollowerId);
        builder.HasIndex(f => f.BlogProfileId);
    }
}
