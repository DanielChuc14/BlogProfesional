using BlogPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/health/db")]
public class HealthController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync(ct);
            return Ok(new
            {
                status = canConnect ? "healthy" : "degraded",
                database = canConnect ? "ok" : "unavailable",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "unhealthy",
                database = "unavailable",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
