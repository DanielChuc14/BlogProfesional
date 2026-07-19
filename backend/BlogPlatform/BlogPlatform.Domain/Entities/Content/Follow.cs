using BlogPlatform.Domain.Entities.Auth;

namespace BlogPlatform.Domain.Entities.Content;

public class Follow
{
    public Guid FollowerId { get; set; }
    public Guid BlogProfileId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ApplicationUser Follower { get; set; } = null!;
    public BlogProfile BlogProfile { get; set; } = null!;
}
