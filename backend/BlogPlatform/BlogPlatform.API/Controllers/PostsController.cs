using System.Security.Claims;
using BlogPlatform.API.Extensions;
using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController(IPostService postService, IServiceScopeFactory scopeFactory) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetFeed(
        [FromQuery] string? cursor,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? tag = null,
        [FromQuery] string? author = null,
        CancellationToken ct = default)
    {
        var query = new PostFeedQuery { Cursor = cursor, PageSize = pageSize, TagSlug = tag, AuthorUsername = author };
        return (await postService.GetFeedAsync(query, ct)).ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await postService.GetByIdAsync(userId.Value, id, ct)).ToActionResult(this);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        // Endpoint publico, pero si llega un Bearer valido User ya esta poblado,
        // lo que permite devolver el estado de "me gusta" del visitante.
        var result = await postService.GetBySlugAsync(slug, GetUserId(), ct);
        if (result.IsSuccess && result.Data is not null)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var ua = Request.Headers.UserAgent.ToString();
            var referrer = Request.Headers.Referer.ToString();
            var postId = result.Data.Id;
            var authorId = result.Data.Author.UserId;

            // El tracking corre fuera de la request para no penalizar la respuesta,
            // por lo que necesita su propio scope de DI: el de la request (y con el
            // su AppDbContext) ya esta liberado cuando esta tarea se ejecuta.
            _ = Task.Run(async () =>
            {
                using var scope = scopeFactory.CreateScope();
                var analytics = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();
                await analytics.TrackPageViewAsync(
                    postId, authorId,
                    ip, ua, string.IsNullOrEmpty(referrer) ? null : referrer,
                    CancellationToken.None);
            });
        }
        return result.ToActionResult(this);
    }

    [HttpPost]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreatePostRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await postService.CreateAsync(userId.Value, request, ct)).ToActionResult(this);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePostRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await postService.UpdateAsync(userId.Value, id, request, ct)).ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await postService.DeleteAsync(userId.Value, id, ct)).ToActionResult(this);
    }

    [HttpPatch("{id:guid}/publish")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await postService.PublishAsync(userId.Value, id, ct)).ToActionResult(this);
    }

    [HttpPatch("{id:guid}/schedule")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> Schedule(Guid id, [FromBody] SchedulePostRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await postService.ScheduleAsync(userId.Value, id, request, ct)).ToActionResult(this);
    }

    [HttpPatch("{id:guid}/archive")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await postService.ArchiveAsync(userId.Value, id, ct)).ToActionResult(this);
    }

    [HttpPatch("{id:guid}/slug")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> ChangeSlug(Guid id, [FromBody] ChangeSlugRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await postService.ChangeSlugAsync(userId.Value, id, request.NewSlug, ct)).ToActionResult(this);
    }

    [HttpPost("{id:guid}/like")]
    [Authorize]
    public async Task<IActionResult> Like(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await postService.LikePostAsync(userId.Value, id, ct)).ToActionResult(this);
    }

    [HttpDelete("{id:guid}/like")]
    [Authorize]
    public async Task<IActionResult> Unlike(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await postService.UnlikePostAsync(userId.Value, id, ct)).ToActionResult(this);
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
