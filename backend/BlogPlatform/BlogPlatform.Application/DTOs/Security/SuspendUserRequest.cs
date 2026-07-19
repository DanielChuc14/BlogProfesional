namespace BlogPlatform.Application.DTOs.Security;

public class SuspendUserRequest
{
    public string Reason { get; set; } = string.Empty;
    public int DurationDays { get; set; }
}
