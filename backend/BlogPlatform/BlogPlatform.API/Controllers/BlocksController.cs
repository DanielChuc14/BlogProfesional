using System.Security.Claims;
using BlogPlatform.API.Extensions;
using BlogPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/blocks")]
[Authorize]
public class BlocksController(IBlockService blockService) : ControllerBase
{
    [HttpPost("{targetUserId:guid}")]
    public async Task<IActionResult> Block(Guid targetUserId, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await blockService.BlockUserAsync(userId.Value, targetUserId, ct)).ToActionResult(this);
    }

    [HttpDelete("{targetUserId:guid}")]
    public async Task<IActionResult> Unblock(Guid targetUserId, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await blockService.UnblockUserAsync(userId.Value, targetUserId, ct)).ToActionResult(this);
    }

    [HttpGet]
    public async Task<IActionResult> GetBlocked(CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await blockService.GetBlockedUsersAsync(userId.Value, page, pageSize, ct)).ToActionResult(this);
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
