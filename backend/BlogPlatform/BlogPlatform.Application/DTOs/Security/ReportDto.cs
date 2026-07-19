namespace BlogPlatform.Application.DTOs.Security;

public class ReportDto
{
    public Guid Id { get; set; }
    public Guid ReporterId { get; set; }
    public string ReporterUsername { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public Guid TargetId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AdminNote { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
