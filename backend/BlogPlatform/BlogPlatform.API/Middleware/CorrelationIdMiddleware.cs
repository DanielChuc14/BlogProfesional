namespace BlogPlatform.API.Middleware;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string CorrelationIdHeader = "X-Correlation-ID";
    public const string CorrelationIdKey = "CorrelationId";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N");

        context.Items[CorrelationIdKey] = correlationId;
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        await next(context);
    }
}
