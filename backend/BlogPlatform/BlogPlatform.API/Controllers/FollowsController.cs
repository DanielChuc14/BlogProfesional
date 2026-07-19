using System.Security.Claims;
using BlogPlatform.API.Extensions;
using BlogPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/users")]
public class FollowsController(IFollowService followService) : ControllerBase
{
    [HttpPost("{profileSlug}/follow")]
    [Authorize]
    public async Task<IActionResult> Follow(string profileSlug, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await followService.FollowAsync(userId.Value, profileSlug, ct)).ToActionResult(this);
    }

    [HttpDelete("{profileSlug}/follow")]
    [Authorize]
    public async Task<IActionResult> Unfollow(string profileSlug, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await followService.UnfollowAsync(userId.Value, profileSlug, ct)).ToActionResult(this);
    }

    [HttpGet("{profileSlug}/followers")]
    public async Task<IActionResult> GetFollowers(string profileSlug, CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => (await followService.GetFollowersAsync(profileSlug, page, pageSize, ct)).ToActionResult(this);

    [HttpGet("{profileSlug}/following")]
    public async Task<IActionResult> GetFollowing(string profileSlug, CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => (await followService.GetFollowingAsync(profileSlug, page, pageSize, ct)).ToActionResult(this);

    private Guid? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
