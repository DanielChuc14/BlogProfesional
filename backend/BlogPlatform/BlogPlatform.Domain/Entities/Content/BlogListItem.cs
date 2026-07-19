namespace BlogPlatform.Domain.Entities.Content;

public class BlogListItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BlogListId { get; set; }
    public Guid PostId { get; set; }
    public int Order { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public BlogList BlogList { get; set; } = null!;
    public Post Post { get; set; } = null!;
}
