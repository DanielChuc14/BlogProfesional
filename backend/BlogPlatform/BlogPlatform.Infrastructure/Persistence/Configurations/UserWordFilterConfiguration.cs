using BlogPlatform.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class UserWordFilterConfiguration : IEntityTypeConfiguration<UserWordFilter>
{
    public void Configure(EntityTypeBuilder<UserWordFilter> builder)
    {
        builder.ToTable("user_word_filters");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Word).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.Word }).IsUnique();
    }
}
