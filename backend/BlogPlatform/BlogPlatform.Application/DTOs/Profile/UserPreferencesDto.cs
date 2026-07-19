namespace BlogPlatform.Application.DTOs.Profile;

public class UserPreferencesDto
{
    public string PreferredLanguage { get; init; } = "en";
    public bool ReceiveEmailNotifications { get; init; } = true;
    public string ProfileVisibility { get; init; } = "public";
}

public class UpdatePreferencesRequest
{
    public bool ReceiveEmailNotifications { get; init; } = true;
    public string ProfileVisibility { get; init; } = "public";
}
