using BlogPlatform.Domain.Entities.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class RestrictedWordConfiguration : IEntityTypeConfiguration<RestrictedWord>
{
    public void Configure(EntityTypeBuilder<RestrictedWord> builder)
    {
        builder.ToTable("restricted_words");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Phrase).HasMaxLength(200).IsRequired();
        builder.Property(x => x.IsRegex).HasDefaultValue(false);
        builder.Property(x => x.Severity);
        builder.HasIndex(x => x.Phrase).IsUnique();
    }
}
