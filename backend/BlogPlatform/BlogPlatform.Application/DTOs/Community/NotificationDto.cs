namespace BlogPlatform.Application.DTOs.Community;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid ActorId { get; set; }
    public string ActorUsername { get; set; } = string.Empty;
    public string ActorDisplayName { get; set; } = string.Empty;
    public string? ActorAvatarUrl { get; set; }
    public Guid? PostId { get; set; }
    public string? PostSlug { get; set; }
    public string? PostTitle { get; set; }
    public Guid? CommentId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
