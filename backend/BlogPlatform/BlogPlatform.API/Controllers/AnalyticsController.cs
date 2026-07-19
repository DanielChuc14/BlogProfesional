using System.Security.Claims;
using BlogPlatform.API.Extensions;
using BlogPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController(IAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet("me")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await analyticsService.GetBloggerDashboardAsync(userId.Value, ct)).ToActionResult(this);
    }

    [HttpGet("posts/{id:guid}")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> GetPostAnalytics(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await analyticsService.GetPostAnalyticsAsync(id, userId.Value, ct)).ToActionResult(this);
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
