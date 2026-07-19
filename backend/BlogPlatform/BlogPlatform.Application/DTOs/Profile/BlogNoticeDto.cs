namespace BlogPlatform.Application.DTOs.Profile;

public class BlogNoticeDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public int Priority { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class CreateBlogNoticeRequest
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Type { get; init; } = "Info";
    public bool IsActive { get; init; } = true;
    public DateTime? ExpiresAt { get; init; }
    public int Priority { get; init; } = 0;
}
