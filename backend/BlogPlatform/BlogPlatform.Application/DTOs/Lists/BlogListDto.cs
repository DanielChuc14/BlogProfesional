using BlogPlatform.Application.DTOs.Posts;

namespace BlogPlatform.Application.DTOs.Lists;

public class BlogListSummaryDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Slug { get; init; } = string.Empty;
    public bool IsPublic { get; init; }
    public string? CoverImageUrl { get; init; }
    public int ItemCount { get; init; }
    public int Order { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class BlogListDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Slug { get; init; } = string.Empty;
    public bool IsPublic { get; init; }
    public string? CoverImageUrl { get; init; }
    public int Order { get; init; }
    public IReadOnlyList<PostSummaryDto> Posts { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public class CreateBlogListRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsPublic { get; init; } = true;
    public string? CoverImageUrl { get; init; }
    public int Order { get; init; } = 0;
}

public class UpdateBlogListRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public bool? IsPublic { get; init; }
    public string? CoverImageUrl { get; init; }
    public int? Order { get; init; }
}

public class AddPostToListRequest
{
    public Guid PostId { get; init; }
    public int Order { get; init; } = 0;
}
