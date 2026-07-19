namespace BlogPlatform.Application.DTOs.Posts;

public class PostTagDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
}

public class PostDetailDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string? Excerpt { get; init; }
    public string? CoverImageUrl { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? PublishedAt { get; init; }
    public DateTime? ScheduledAt { get; init; }
    public int ReadTimeMinutes { get; init; }
    public int ViewCount { get; init; }
    public int LikesCount { get; init; }
    public int CommentsCount { get; init; }
    public bool IsAdultContent { get; init; }
    public IReadOnlyList<PostTagDto> Tags { get; init; } = [];
    public AuthorDto Author { get; init; } = null!;
    public PostSeoDto? Seo { get; init; }
    public IReadOnlyList<PostMediaDto> Media { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public class PostSeoDto
{
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public string? OgImageUrl { get; init; }
    public string? CanonicalUrl { get; init; }
}

public class PostMediaDto
{
    public Guid Id { get; init; }
    public string Url { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string? Caption { get; init; }
    public int SortOrder { get; init; }
}
