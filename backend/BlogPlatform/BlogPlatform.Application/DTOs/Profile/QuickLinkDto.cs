namespace BlogPlatform.Application.DTOs.Profile;

public class QuickLinkDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string? Icon { get; init; }
    public int Order { get; init; }
}

public class CreateQuickLinkRequest
{
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string? Icon { get; init; }
    public int Order { get; init; } = 0;
}
