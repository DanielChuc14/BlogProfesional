namespace BlogPlatform.Application.DTOs.Admin;

public class AdminPostSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AuthorUsername { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
