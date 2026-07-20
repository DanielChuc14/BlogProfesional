namespace BlogPlatform.API.Middleware;

// Adds baseline security response headers to every request.
// La CSP estricta se omite en las rutas de Swagger (solo Development), ya que
// Swagger UI usa estilos y scripts inline que una CSP restrictiva bloquearia.
// La CSP que protege la SPA de Angular se define en el reverse proxy (Caddy).
public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";
        headers["Cross-Origin-Opener-Policy"] = "same-origin";

        var isSwagger = context.Request.Path.StartsWithSegments("/swagger");
        if (!isSwagger)
        {
            // La API sirve JSON; no necesita cargar recursos de ningun origen.
            headers["Content-Security-Policy"] =
                "default-src 'none'; frame-ancestors 'none'; base-uri 'none'";
        }

        await next(context);
    }
}
