namespace BlogPlatform.Application.DTOs.Admin;

public class UpdateSettingsRequest
{
    public Dictionary<string, string> Settings { get; set; } = new();
}
