using BlogPlatform.Domain.Entities.Auth;
using BlogPlatform.Domain.Enums;

namespace BlogPlatform.Domain.Entities.Content;

public class Notification : Common.BaseEntity
{
    public Guid RecipientId { get; set; }
    public Guid ActorId { get; set; }
    public NotificationType Type { get; set; }
    public Guid? PostId { get; set; }
    public Guid? CommentId { get; set; }
    public bool IsRead { get; set; }

    // Navigation
    public ApplicationUser Recipient { get; set; } = null!;
    public ApplicationUser Actor { get; set; } = null!;
    public Post? Post { get; set; }
    public Comment? Comment { get; set; }
}
