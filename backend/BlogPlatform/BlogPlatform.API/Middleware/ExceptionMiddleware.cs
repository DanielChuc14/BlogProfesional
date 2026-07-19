using System.Text.Json;
using FluentValidation;

namespace BlogPlatform.API.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            var correlationId = context.Items[CorrelationIdMiddleware.CorrelationIdKey]?.ToString();
            logger.LogWarning("Validation failed [{CorrelationId}]: {Errors}", correlationId, ex.Message);
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/problem+json";
            var errors = ex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage });
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                title = "Validation failed",
                status = 400,
                correlationId,
                errors
            }));
        }
        catch (Exception ex)
        {
            var correlationId = context.Items[CorrelationIdMiddleware.CorrelationIdKey]?.ToString();
            logger.LogError(ex, "Unhandled exception [{CorrelationId}]", correlationId);
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                title = "An unexpected error occurred.",
                status = 500,
                correlationId
            }));
        }
    }
}
