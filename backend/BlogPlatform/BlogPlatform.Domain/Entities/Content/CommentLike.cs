using BlogPlatform.Domain.Entities.Auth;

namespace BlogPlatform.Domain.Entities.Content;

public class CommentLike
{
    public Guid CommentId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Comment Comment { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
