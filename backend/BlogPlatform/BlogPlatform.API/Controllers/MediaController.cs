using System.Security.Claims;
using BlogPlatform.API.Extensions;
using BlogPlatform.Application.DTOs.Media;
using BlogPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/media")]
[Authorize(Roles = "Blogger,Admin,SuperAdmin")]
public class MediaController(IMediaService mediaService) : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file provided." });

        var dto = new FileUploadDto(file.OpenReadStream(), file.FileName, file.ContentType, file.Length);
        return (await mediaService.UploadAsync(userId.Value, dto, ct)).ToActionResult(this);
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
