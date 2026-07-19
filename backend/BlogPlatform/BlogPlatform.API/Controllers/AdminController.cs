using System.Security.Claims;
using BlogPlatform.API.Extensions;
using BlogPlatform.Application.DTOs.Admin;
using BlogPlatform.Application.DTOs.Security;
using BlogPlatform.Application.DTOs.Tags;
using BlogPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminController(
    IAdminService adminService,
    IAnalyticsService analyticsService,
    IPlatformSettingsService platformSettings,
    IReportService reportService,
    ILanguageService languageService) : ControllerBase
{
    // ── Analytics ─────────────────────────────────────────────────────────────

    [HttpGet("analytics")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAdminDashboard(CancellationToken ct)
        => (await analyticsService.GetAdminDashboardAsync(ct)).ToActionResult(this);

    // ── Users ─────────────────────────────────────────────────────────────────

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => (await adminService.GetUsersAsync(page, pageSize, search, ct)).ToActionResult(this);

    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken ct)
        => (await adminService.GetUserByIdAsync(id, ct)).ToActionResult(this);

    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
        => (await adminService.DeleteUserAsync(id, ct)).ToActionResult(this);

    [HttpPatch("users/{id:guid}/ban")]
    public async Task<IActionResult> BanUser(Guid id, CancellationToken ct)
        => (await adminService.BanUserAsync(id, ct)).ToActionResult(this);

    [HttpPatch("users/{id:guid}/unban")]
    public async Task<IActionResult> UnbanUser(Guid id, CancellationToken ct)
        => (await adminService.UnbanUserAsync(id, ct)).ToActionResult(this);

    [HttpPost("users/{id:guid}/suspend")]
    public async Task<IActionResult> SuspendUser(Guid id, [FromBody] SuspendUserRequest req, CancellationToken ct)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return (await adminService.SuspendUserAsync(adminId, id, req, ct)).ToActionResult(this);
    }

    [HttpDelete("users/{id:guid}/suspend")]
    public async Task<IActionResult> LiftSuspension(Guid id, CancellationToken ct)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return (await adminService.LiftSuspensionAsync(adminId, id, ct)).ToActionResult(this);
    }

    [HttpGet("users/{id:guid}/suspensions")]
    public async Task<IActionResult> GetSuspensionHistory(Guid id, CancellationToken ct)
        => (await adminService.GetSuspensionHistoryAsync(id, ct)).ToActionResult(this);

    [HttpPut("users/{id:guid}/role")]
    public async Task<IActionResult> ChangeUserRole(Guid id, [FromBody] AssignRoleRequest req, CancellationToken ct)
    {
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var allowedRoles = isSuperAdmin
            ? new[] { "Reader", "Blogger", "Admin" }
            : new[] { "Reader", "Blogger" };

        if (!allowedRoles.Contains(req.Role))
            return BadRequest(new { error = $"Role must be one of: {string.Join(", ", allowedRoles)}." });

        var userResult = await adminService.GetUserByIdAsync(id, ct);
        if (!userResult.IsSuccess)
            return userResult.ToActionResult(this);

        foreach (var currentRole in userResult.Data!.Roles)
        {
            if (currentRole is "Reader" or "Blogger" or "Admin")
                await adminService.RemoveRoleAsync(id, currentRole, ct);
        }

        return (await adminService.AssignRoleAsync(id, req.Role, ct)).ToActionResult(this);
    }

    [HttpPost("users/{id:guid}/roles")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> AssignRole(Guid id, [FromBody] AssignRoleRequest req, CancellationToken ct)
        => (await adminService.AssignRoleAsync(id, req.Role, ct)).ToActionResult(this);

    [HttpDelete("users/{id:guid}/roles/{role}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> RemoveRole(Guid id, string role, CancellationToken ct)
        => (await adminService.RemoveRoleAsync(id, role, ct)).ToActionResult(this);

    // ── Roles ─────────────────────────────────────────────────────────────────

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
        => (await adminService.GetRolesAsync(ct)).ToActionResult(this);

    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole([FromBody] AssignRoleRequest req, CancellationToken ct)
        => (await adminService.CreateRoleAsync(req.Role, ct)).ToActionResult(this);

    [HttpDelete("roles/{name}")]
    public async Task<IActionResult> DeleteRole(string name, CancellationToken ct)
        => (await adminService.DeleteRoleAsync(name, ct)).ToActionResult(this);

    // ── Posts ─────────────────────────────────────────────────────────────────

    [HttpGet("posts")]
    public async Task<IActionResult> GetPosts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
        => (await adminService.GetAllPostsAsync(page, pageSize, status, ct)).ToActionResult(this);

    [HttpPatch("posts/{id:guid}/unpublish")]
    public async Task<IActionResult> UnpublishPost(Guid id, [FromBody] UnpublishRequest req, CancellationToken ct)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return (await adminService.UnpublishPostAsync(adminId, id, req.Reason, ct)).ToActionResult(this);
    }

    [HttpDelete("posts/{id:guid}")]
    public async Task<IActionResult> DeletePost(Guid id, CancellationToken ct)
        => (await adminService.ForceDeletePostAsync(id, ct)).ToActionResult(this);

    // ── Comments ──────────────────────────────────────────────────────────────

    [HttpGet("comments")]
    public async Task<IActionResult> GetComments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => (await adminService.GetCommentsAsync(page, pageSize, ct)).ToActionResult(this);

    [HttpDelete("comments/{id:guid}")]
    public async Task<IActionResult> DeleteComment(Guid id, CancellationToken ct)
        => (await adminService.DeleteCommentAsync(id, ct)).ToActionResult(this);

    // ── Platform Settings ─────────────────────────────────────────────────────

    [HttpGet("settings")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
        => Ok(await platformSettings.GetAllAsync(ct));

    [HttpPut("settings")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest req, CancellationToken ct)
    {
        if (req.Settings is null || req.Settings.Count == 0)
            return BadRequest(new { error = "No settings provided." });

        var actorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await platformSettings.UpsertManyAsync(req.Settings, actorId, ct);
        return NoContent();
    }

    // ── Tags ──────────────────────────────────────────────────────────────────

    [HttpGet("tags")]
    public async Task<IActionResult> GetTags(CancellationToken ct)
        => (await adminService.GetTagsAsync(ct)).ToActionResult(this);

    [HttpPost("tags")]
    public async Task<IActionResult> CreateTag([FromBody] TagRequest req, CancellationToken ct)
        => (await adminService.CreateTagAsync(req.Name, ct)).ToActionResult(this);

    [HttpPut("tags/{id:guid}")]
    public async Task<IActionResult> UpdateTag(Guid id, [FromBody] TagRequest req, CancellationToken ct)
        => (await adminService.UpdateTagAsync(id, req.Name, ct)).ToActionResult(this);

    [HttpDelete("tags/{id:guid}")]
    public async Task<IActionResult> DeleteTag(Guid id, CancellationToken ct)
        => (await adminService.DeleteTagAsync(id, ct)).ToActionResult(this);

    // ── Restricted Words ───────────────────────────────────────────────────────

    [HttpGet("restricted-words")]
    public async Task<IActionResult> GetRestrictedWords(CancellationToken ct)
        => (await adminService.GetRestrictedWordsAsync(ct)).ToActionResult(this);

    [HttpPost("restricted-words")]
    public async Task<IActionResult> AddRestrictedWord([FromBody] AddRestrictedWordRequest req, CancellationToken ct)
    {
        var actorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return (await adminService.AddRestrictedWordAsync(actorId, req, ct)).ToActionResult(this);
    }

    [HttpDelete("restricted-words/{id:guid}")]
    public async Task<IActionResult> DeleteRestrictedWord(Guid id, CancellationToken ct)
        => (await adminService.DeleteRestrictedWordAsync(id, ct)).ToActionResult(this);

    // ── Audit logs ─────────────────────────────────────────────────────────────

    [HttpGet("audit-logs")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken ct = default)
        => (await adminService.GetAuditLogsAsync(page, pageSize, ct)).ToActionResult(this);

    // ── Reports ────────────────────────────────────────────────────────────────

    [HttpGet("reports")]
    public async Task<IActionResult> GetReports(
        [FromQuery] string? status = null,
        [FromQuery] string? targetType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => (await reportService.GetReportsAsync(status, targetType, page, pageSize, ct)).ToActionResult(this);

    [HttpGet("reports/{id:guid}")]
    public async Task<IActionResult> GetReport(Guid id, CancellationToken ct)
        => (await reportService.GetReportByIdAsync(id, ct)).ToActionResult(this);

    [HttpPut("reports/{id:guid}/review")]
    public async Task<IActionResult> ReviewReport(Guid id, [FromBody] ReviewReportRequest req, CancellationToken ct)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return (await reportService.ReviewReportAsync(adminId, id, req, ct)).ToActionResult(this);
    }

    // ── Languages ─────────────────────────────────────────────────────────────

    [HttpGet("languages")]
    public async Task<IActionResult> GetLanguages(CancellationToken ct)
        => (await languageService.GetAllLanguagesAsync(ct)).ToActionResult(this);

    [HttpPost("languages")]
    public async Task<IActionResult> CreateLanguage([FromBody] CreateLanguageRequest req, CancellationToken ct)
        => (await languageService.CreateLanguageAsync(req, ct)).ToActionResult(this);

    [HttpPut("languages/{code}")]
    public async Task<IActionResult> UpdateLanguage(string code, [FromBody] UpdateLanguageAdminRequest req, CancellationToken ct)
        => (await languageService.UpdateLanguageAsync(code, req, ct)).ToActionResult(this);

    [HttpPost("languages/{code}/translations")]
    public async Task<IActionResult> UploadTranslation(string code, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file provided." });

        if (!file.ContentType.Contains("json") && !file.FileName.EndsWith(".json"))
            return BadRequest(new { error = "Only JSON files are accepted." });

        using var reader = new StreamReader(file.OpenReadStream());
        var json = await reader.ReadToEndAsync(ct);

        return (await languageService.UploadTranslationAsync(code, json, ct)).ToActionResult(this);
    }

    [HttpPatch("languages/{code}/toggle")]
    public async Task<IActionResult> ToggleLanguage(string code, CancellationToken ct)
        => (await languageService.ToggleLanguageAsync(code, ct)).ToActionResult(this);

    [HttpDelete("languages/{code}")]
    public async Task<IActionResult> DeleteLanguage(string code, CancellationToken ct)
        => (await languageService.DeleteLanguageAsync(code, ct)).ToActionResult(this);
}
