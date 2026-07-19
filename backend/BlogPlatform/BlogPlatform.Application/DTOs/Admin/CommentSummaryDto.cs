namespace BlogPlatform.Application.DTOs.Admin;

public class CommentSummaryDto
{
    public Guid Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public string AuthorUsername { get; set; } = string.Empty;
    public string PostTitle { get; set; } = string.Empty;
    public string PostSlug { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public int LikesCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
