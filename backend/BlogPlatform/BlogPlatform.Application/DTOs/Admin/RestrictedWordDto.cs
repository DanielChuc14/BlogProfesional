namespace BlogPlatform.Application.DTOs.Admin;

public class RestrictedWordDto
{
    public Guid Id { get; init; }
    public string Phrase { get; init; } = string.Empty;
    public bool IsRegex { get; init; }
    public string Severity { get; init; } = "Block";
    public DateTime CreatedAt { get; init; }
}

public class AddRestrictedWordRequest
{
    public string Phrase { get; init; } = string.Empty;
    public bool IsRegex { get; init; } = false;
    public string Severity { get; init; } = "Block";
}

public class AuditLogDto
{
    public Guid Id { get; init; }
    public Guid ActorId { get; init; }
    public string ActorUsername { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public string? EntityId { get; init; }
    public string? Reason { get; init; }
    public DateTime CreatedAt { get; init; }
}
