namespace BlogPlatform.Application.DTOs.Profile;

public class BlogThemeDto
{
    public string? PrimaryColor { get; init; }
    public string? SecondaryColor { get; init; }
    public string? AccentColor { get; init; }
    public string? FontFamily { get; init; }
    public string? LayoutStyle { get; init; }
    public bool DarkModeDefault { get; init; } = false;
}

public class UpdateBlogThemeRequest
{
    public string? PrimaryColor { get; init; }
    public string? SecondaryColor { get; init; }
    public string? AccentColor { get; init; }
    public string? FontFamily { get; init; }
    public string? LayoutStyle { get; init; }
    public bool DarkModeDefault { get; init; } = false;
}
