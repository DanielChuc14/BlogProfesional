namespace BlogPlatform.Application.DTOs.Admin;

public class PlatformSettingDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
