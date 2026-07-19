using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BlogPlatform.Application.Interfaces;
using BlogPlatform.Domain.Entities.Admin;
using BlogPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Infrastructure.Services;

public class AuditService(
    AppDbContext db,
    IHttpContextAccessor httpContextAccessor,
    ILogger<AuditService> logger) : IAuditService
{
    public async Task LogAsync(
        string action,
        string entityType,
        string? entityId = null,
        string? reason = null,
        CancellationToken ct = default)
    {
        var ctx = httpContextAccessor.HttpContext;
        var userIdStr = ctx?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid.TryParse(userIdStr, out var actorId);

        var ip = ctx?.Connection.RemoteIpAddress?.ToString();
        string? ipHash = null;
        if (ip is not null)
            ipHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(ip))).ToLower();

        db.AuditLogs.Add(new AuditLog
        {
            ActorId = actorId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Reason = reason,
            IpHash = ipHash
        });

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to persist audit log: {Action} on {EntityType}/{EntityId}", action, entityType, entityId);
        }
    }
}
