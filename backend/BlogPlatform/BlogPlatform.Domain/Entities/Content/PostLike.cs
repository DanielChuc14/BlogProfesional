using BlogPlatform.Domain.Entities.Auth;

namespace BlogPlatform.Domain.Entities.Content;

public class PostLike
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Post Post { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
