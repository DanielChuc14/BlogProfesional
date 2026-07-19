using BlogPlatform.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.API.Extensions;

public static class ResultModelExtensions
{
    public static IActionResult ToActionResult<T>(this ResultModel<T> result, ControllerBase controller)
    {
        return result.StatusCode switch
        {
            200 => controller.Ok(result.Data),
            201 => controller.StatusCode(201, result.Data),
            204 => controller.NoContent(),
            400 => controller.BadRequest(new { error = result.Error }),
            401 => controller.Unauthorized(new { error = result.Error }),
            403 => controller.StatusCode(403, new { error = result.Error }),
            404 => controller.NotFound(new { error = result.Error }),
            409 => controller.Conflict(new { error = result.Error }),
            500 => controller.StatusCode(500, new { error = result.Error }),
            _ => controller.StatusCode(result.StatusCode, new { error = result.Error })
        };
    }

    public static IActionResult ToActionResult(this ResultModel result, ControllerBase controller)
    {
        return result.StatusCode switch
        {
            204 => controller.NoContent(),
            400 => controller.BadRequest(new { error = result.Error }),
            401 => controller.Unauthorized(new { error = result.Error }),
            403 => controller.StatusCode(403, new { error = result.Error }),
            404 => controller.NotFound(new { error = result.Error }),
            409 => controller.Conflict(new { error = result.Error }),
            500 => controller.StatusCode(500, new { error = result.Error }),
            _ => controller.StatusCode(result.StatusCode, new { error = result.Error })
        };
    }
}
