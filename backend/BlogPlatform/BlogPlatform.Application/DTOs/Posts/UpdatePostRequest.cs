namespace BlogPlatform.Application.DTOs.Posts;

public class UpdatePostRequest
{
    public string? Title { get; init; }
    public string? Body { get; init; }
    public string? Excerpt { get; init; }
    public string? CoverImageUrl { get; init; }
    public bool? IsAdultContent { get; init; }
    public IReadOnlyList<Guid>? TagIds { get; init; }
    public PostSeoRequest? Seo { get; init; }
}
