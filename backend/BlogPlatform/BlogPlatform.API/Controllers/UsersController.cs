using System.Security.Claims;
using BlogPlatform.API.Extensions;
using BlogPlatform.Application.DTOs.Media;
using BlogPlatform.Application.DTOs.Profile;
using BlogPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IProfileService profileService) : ControllerBase
{
    [HttpGet("{username}")]
    public async Task<IActionResult> GetByUsername(string username, CancellationToken ct)
        => (await profileService.GetByUsernameAsync(username, GetUserId(), ct)).ToActionResult(this);

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await profileService.UpdateAsync(userId.Value, request, ct)).ToActionResult(this);
    }

    [HttpPut("me/avatar")]
    [Authorize]
    public async Task<IActionResult> UpdateAvatar(IFormFile file, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file provided." });

        var dto = new FileUploadDto(file.OpenReadStream(), file.FileName, file.ContentType, file.Length);
        return (await profileService.UpdateAvatarAsync(userId.Value, dto, ct)).ToActionResult(this);
    }

    [HttpPut("me/banner")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateBanner(IFormFile file, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file provided." });

        var dto = new FileUploadDto(file.OpenReadStream(), file.FileName, file.ContentType, file.Length);
        return (await profileService.UpdateBannerAsync(userId.Value, dto, ct)).ToActionResult(this);
    }

    // ── Theme ──────────────────────────────────────────────────────────────

    [HttpGet("me/theme")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> GetTheme(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await profileService.GetThemeAsync(userId.Value, ct)).ToActionResult(this);
    }

    [HttpPut("me/theme")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateTheme([FromBody] UpdateBlogThemeRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await profileService.UpdateThemeAsync(userId.Value, request, ct)).ToActionResult(this);
    }

    // ── Word Filters ───────────────────────────────────────────────────────

    [HttpGet("me/word-filters")]
    [Authorize]
    public async Task<IActionResult> GetWordFilters(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await profileService.GetWordFiltersAsync(userId.Value, ct)).ToActionResult(this);
    }

    [HttpPost("me/word-filters")]
    [Authorize]
    public async Task<IActionResult> AddWordFilter([FromBody] AddWordFilterRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await profileService.AddWordFilterAsync(userId.Value, request, ct)).ToActionResult(this);
    }

    [HttpDelete("me/word-filters/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteWordFilter(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await profileService.DeleteWordFilterAsync(userId.Value, id, ct)).ToActionResult(this);
    }

    // ── Notices ────────────────────────────────────────────────────────────

    [HttpGet("me/notices")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> GetNotices(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await profileService.GetNoticesAsync(userId.Value, ct)).ToActionResult(this);
    }

    [HttpPost("me/notices")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> AddNotice([FromBody] CreateBlogNoticeRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await profileService.AddNoticeAsync(userId.Value, request, ct)).ToActionResult(this);
    }

    [HttpPut("me/notices/{id:guid}")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateNotice(Guid id, [FromBody] CreateBlogNoticeRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await profileService.UpdateNoticeAsync(userId.Value, id, request, ct)).ToActionResult(this);
    }

    [HttpDelete("me/notices/{id:guid}")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteNotice(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await profileService.DeleteNoticeAsync(userId.Value, id, ct)).ToActionResult(this);
    }

    // ── Quick Links ────────────────────────────────────────────────────────

    [HttpGet("me/quick-links")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> GetQuickLinks(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await profileService.GetQuickLinksAsync(userId.Value, ct)).ToActionResult(this);
    }

    [HttpPost("me/quick-links")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> AddQuickLink([FromBody] CreateQuickLinkRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await profileService.AddQuickLinkAsync(userId.Value, request, ct)).ToActionResult(this);
    }

    [HttpDelete("me/quick-links/{id:guid}")]
    [Authorize(Roles = "Blogger,Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteQuickLink(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await profileService.DeleteQuickLinkAsync(userId.Value, id, ct)).ToActionResult(this);
    }

    [HttpGet("me/preferences")]
    [Authorize]
    public async Task<IActionResult> GetPreferences(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await profileService.GetPreferencesAsync(userId.Value, ct)).ToActionResult(this);
    }

    [HttpPut("me/preferences")]
    [Authorize]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await profileService.UpdatePreferencesAsync(userId.Value, request, ct)).ToActionResult(this);
    }

    [HttpPut("me/language")]
    [Authorize]
    public async Task<IActionResult> UpdateLanguage([FromBody] UpdateLanguageRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await profileService.UpdateLanguageAsync(userId.Value, request.Language, ct)).ToActionResult(this);
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
