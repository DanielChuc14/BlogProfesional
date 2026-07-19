using System.Security.Claims;
using BlogPlatform.API.Extensions;
using BlogPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(INotificationService notificationService) : ControllerBase
{
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await notificationService.GetUnreadCountAsync(userId.Value, ct)).ToActionResult(this);
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? cursor,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await notificationService.GetAsync(userId.Value, cursor, pageSize, ct)).ToActionResult(this);
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await notificationService.MarkReadAsync(userId.Value, id, ct)).ToActionResult(this);
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await notificationService.MarkAllReadAsync(userId.Value, ct)).ToActionResult(this);
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
