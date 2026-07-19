namespace BlogPlatform.Application.DTOs.Community;

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid? ParentId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public string AuthorDisplayName { get; set; } = string.Empty;
    public string? AuthorAvatarUrl { get; set; }
    public string Body { get; set; } = string.Empty;
    public int LikesCount { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<CommentDto> Replies { get; set; } = [];
}
