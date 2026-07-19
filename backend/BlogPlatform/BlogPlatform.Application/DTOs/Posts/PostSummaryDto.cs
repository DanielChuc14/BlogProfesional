namespace BlogPlatform.Application.DTOs.Posts;

public class PostSummaryDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Excerpt { get; init; }
    public string? CoverImageUrl { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? PublishedAt { get; init; }
    public int ReadTimeMinutes { get; init; }
    public int ViewCount { get; init; }
    public int LikesCount { get; init; }
    public int CommentsCount { get; init; }
    public bool IsAdultContent { get; init; }
    public bool IsFeatured { get; init; }
    public IReadOnlyList<PostTagDto> Tags { get; init; } = [];
    public AuthorDto Author { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}

public class AuthorDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
}
