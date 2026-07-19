namespace BlogPlatform.Application.DTOs.Posts;

public class CreatePostRequest
{
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string? Excerpt { get; init; }
    public string? CoverImageUrl { get; init; }
    public bool IsAdultContent { get; init; } = false;
    public IReadOnlyList<Guid> TagIds { get; init; } = [];
    public PostSeoRequest? Seo { get; init; }
}

public class PostSeoRequest
{
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public string? OgImageUrl { get; init; }
    public string? CanonicalUrl { get; init; }
}
