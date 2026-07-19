using BlogPlatform.API.Extensions;
using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController(IPostService postService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] string? cursor,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new SearchQuery { Term = q ?? string.Empty, Cursor = cursor, PageSize = pageSize };
        return (await postService.SearchAsync(query, ct)).ToActionResult(this);
    }
}
