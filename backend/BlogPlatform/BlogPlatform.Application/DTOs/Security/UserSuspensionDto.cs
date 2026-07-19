namespace BlogPlatform.Application.DTOs.Security;

public class UserSuspensionDto
{
    public Guid Id { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public string SuspendedByUsername { get; set; } = string.Empty;
    public string? LiftedByUsername { get; set; }
    public DateTime? LiftedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
