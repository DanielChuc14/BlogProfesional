using System.Security.Claims;
using BlogPlatform.API.Extensions;
using BlogPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/feed")]
[Authorize]
public class FeedController(IFeedService feedService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetFeed(
        [FromQuery] string? cursor,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool includeAdultContent = false,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await feedService.GetPersonalizedFeedAsync(userId.Value, cursor, pageSize, includeAdultContent, ct)).ToActionResult(this);
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
