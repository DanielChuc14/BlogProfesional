using System.Security.Claims;
using BlogPlatform.API.Extensions;
using BlogPlatform.Application.DTOs.Community;
using BlogPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api")]
public class CommentsController(ICommentService commentService) : ControllerBase
{
    [HttpGet("posts/{postId:guid}/comments")]
    public async Task<IActionResult> GetByPost(Guid postId, CancellationToken ct)
        => (await commentService.GetByPostAsync(postId, ct)).ToActionResult(this);

    [HttpPost("posts/{postId:guid}/comments")]
    [Authorize]
    public async Task<IActionResult> Create(Guid postId, [FromBody] CreateCommentRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await commentService.CreateAsync(userId.Value, postId, request, ct)).ToActionResult(this);
    }

    [HttpPut("comments/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCommentRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await commentService.UpdateAsync(userId.Value, id, request, ct)).ToActionResult(this);
    }

    [HttpDelete("comments/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await commentService.DeleteAsync(userId.Value, id, ct)).ToActionResult(this);
    }

    [HttpPost("comments/{id:guid}/like")]
    [Authorize]
    public async Task<IActionResult> Like(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await commentService.LikeAsync(userId.Value, id, ct)).ToActionResult(this);
    }

    [HttpDelete("comments/{id:guid}/like")]
    [Authorize]
    public async Task<IActionResult> Unlike(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await commentService.UnlikeAsync(userId.Value, id, ct)).ToActionResult(this);
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
