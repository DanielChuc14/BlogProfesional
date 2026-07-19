namespace BlogPlatform.Application.DTOs.Security;

public class ReviewReportRequest
{
    public string Decision { get; set; } = string.Empty; // "Resolved" | "Rejected"
    public string? Note { get; set; }
}
