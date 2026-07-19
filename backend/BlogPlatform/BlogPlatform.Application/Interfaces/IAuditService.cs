namespace BlogPlatform.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, string? entityId = null, string? reason = null, CancellationToken ct = default);
}
