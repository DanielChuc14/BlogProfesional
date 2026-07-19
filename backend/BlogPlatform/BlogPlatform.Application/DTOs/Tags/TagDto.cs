namespace BlogPlatform.Application.DTOs.Tags;

public class TagDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public int PostsCount { get; init; }
}
