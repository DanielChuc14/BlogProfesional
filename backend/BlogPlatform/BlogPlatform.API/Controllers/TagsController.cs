using BlogPlatform.API.Extensions;
using BlogPlatform.Application.DTOs.Tags;
using BlogPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/tags")]
public class TagsController(ITagService tagService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => (await tagService.GetAllAsync(ct)).ToActionResult(this);

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] TagRequest req, CancellationToken ct)
        => (await tagService.CreateAsync(req.Name, ct)).ToActionResult(this);

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TagRequest req, CancellationToken ct)
        => (await tagService.UpdateAsync(id, req.Name, ct)).ToActionResult(this);

    [HttpGet("autocomplete")]
    [Authorize]
    public async Task<IActionResult> Autocomplete([FromQuery] string q, CancellationToken ct)
        => (await tagService.AutocompleteAsync(q ?? string.Empty, ct)).ToActionResult(this);

    [HttpGet("{slug}/posts")]
    public async Task<IActionResult> GetPostsByTag(
        string slug,
        [FromQuery] string? cursor,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => (await tagService.GetPostsByTagAsync(slug, cursor, pageSize, ct)).ToActionResult(this);
}
