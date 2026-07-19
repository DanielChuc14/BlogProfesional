using BlogPlatform.Domain.Enums;

namespace BlogPlatform.Application.DTOs.Security;

public class CreateReportRequest
{
    public ReportTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }
    public ReportReason Reason { get; set; }
    public string? Description { get; set; }
}
