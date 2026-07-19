using BlogPlatform.Domain.Entities.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

public class NewsletterSendConfiguration : IEntityTypeConfiguration<NewsletterSend>
{
    public void Configure(EntityTypeBuilder<NewsletterSend> builder)
    {
        builder.ToTable("newsletter_sends");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Subject).HasMaxLength(300).IsRequired();
        builder.Property(n => n.HtmlBody).IsRequired();
        builder.Property(n => n.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(n => n.BlogProfile)
            .WithMany()
            .HasForeignKey(n => n.BlogProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => new { n.BlogProfileId, n.CreatedAt });
    }
}
