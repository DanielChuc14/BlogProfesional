namespace BlogPlatform.Application.DTOs.Posts;

public class PostFeedQuery
{
    public string? Cursor { get; init; }
    public int PageSize { get; init; } = 20;
    public string? TagSlug { get; init; }
    public string? AuthorUsername { get; init; }
    public bool IncludeAdultContent { get; init; } = false;
}
