namespace BlogPlatform.Domain.Entities.Content;

public class SlugRedirect
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OldSlug { get; set; } = string.Empty;
    public Guid PostId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Post Post { get; set; } = null!;
}
