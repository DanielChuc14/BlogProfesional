namespace BlogPlatform.Application.DTOs.Posts;

public class SearchQuery
{
    public string Term { get; init; } = string.Empty;
    public string? Cursor { get; init; }
    public int PageSize { get; init; } = 20;
}
