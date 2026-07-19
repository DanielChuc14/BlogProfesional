namespace BlogPlatform.Application.DTOs.Auth;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public IEnumerable<string> Roles { get; set; } = [];
    public string? RefreshToken { get; set; }
    public string? AvatarUrl { get; set; }
    public string PreferredLanguage { get; set; } = "en";
}
