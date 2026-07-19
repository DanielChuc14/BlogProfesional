using System.Security.Claims;
using BlogPlatform.API.Extensions;
using BlogPlatform.Application.DTOs.Security;
using BlogPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReportRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await reportService.CreateReportAsync(userId.Value, request, ct)).ToActionResult(this);
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return (await reportService.GetMyReportsAsync(userId.Value, page, pageSize, ct)).ToActionResult(this);
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
