using System.Security.Claims;
using BlogPlatform.API.Extensions;
using BlogPlatform.Application.DTOs.Lists;
using BlogPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/lists")]
public class ListsController(IBlogListService listService) : ControllerBase
{
    [HttpGet("{username}")]
    public async Task<IActionResult> GetByProfile(string username, CancellationToken ct)
    {
        var isOwner = User.Identity?.IsAuthenticated == true &&
                      User.FindFirstValue(ClaimTypes.Name) == username;
        return (await listService.GetByProfileAsync(username, isOwner, ct)).ToActionResult(this);
    }

    [HttpGet("{username}/{slug}")]
    public async Task<IActionResult> GetBySlug(string username, string slug, CancellationToken ct)
        => (await listService.GetBySlugAsync(username, slug, ct)).ToActionResult(this);

    [HttpPost]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateBlogListRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await listService.CreateAsync(userId.Value, request, ct)).ToActionResult(this);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBlogListRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await listService.UpdateAsync(userId.Value, id, request, ct)).ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await listService.DeleteAsync(userId.Value, id, ct)).ToActionResult(this);
    }

    [HttpPost("{id:guid}/posts")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> AddPost(Guid id, [FromBody] AddPostToListRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await listService.AddPostAsync(userId.Value, id, request, ct)).ToActionResult(this);
    }

    [HttpDelete("{id:guid}/posts/{postId:guid}")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> RemovePost(Guid id, Guid postId, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await listService.RemovePostAsync(userId.Value, id, postId, ct)).ToActionResult(this);
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
