using System.Text;
using System.Threading.RateLimiting;
using BlogPlatform.API.HealthChecks;
using BlogPlatform.API.Middleware;
using BlogPlatform.Domain.Entities.Auth;
using BlogPlatform.Infrastructure;
using BlogPlatform.Infrastructure.Persistence;
using BlogPlatform.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();



builder.Services.AddAuthentication(opts =>
{
    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opts =>
{
    opts.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
    };
});

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("RequireSuperAdmin", p => p.RequireRole("SuperAdmin"));
    opts.AddPolicy("RequireAdmin", p => p.RequireRole("SuperAdmin", "Admin"));
    opts.AddPolicy("RequireBlogger", p => p.RequireRole("SuperAdmin", "Admin", "Blogger"));
});


// Health Checks
var healthChecks = builder.Services.AddHealthChecks();

var pgConn = builder.Configuration.GetConnectionString("Default");
if (!string.IsNullOrWhiteSpace(pgConn))
    healthChecks.AddNpgSql(pgConn, name: "postgres", tags: ["db"]);

var redisConn = builder.Configuration["Redis:ConnectionString"];
if (!string.IsNullOrWhiteSpace(redisConn))
    healthChecks.AddRedis(redisConn, name: "redis", tags: ["cache"]);

var storagePath = builder.Configuration["Storage:BasePath"] ?? "/uploads";
healthChecks.Add(new HealthCheckRegistration("storage", _ =>
    new StorageHealthCheck(storagePath), HealthStatus.Degraded, ["storage"]));
//
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
{
    opts.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BlogPlatform API",
        Version = "v1",
        Description = "REST API for BlogPlatform"
    });

    opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    opts.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

builder.Services.AddCors(opts =>
{
    opts.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

builder.Services.AddRateLimiter(opts =>
{
    opts.RejectionStatusCode = 429;
    opts.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.Headers["Retry-After"] = "60";
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { error = "Too many requests. Please try again later.", retryAfterSeconds = 60 }, token);
    };

    // Fixed Window: 5 intentos de login por IP cada 15 minutos
    opts.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(15),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Sliding Window: 100 req/min por usuario autenticado
    opts.AddPolicy("authenticated", httpContext =>
    {
        var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "anonymous";
        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: userId,
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opts =>
    {
        opts.SwaggerEndpoint("/swagger/v1/swagger.json", "BlogPlatform API v1");
        opts.RoutePrefix = "swagger";
    });
}

var uploadsPhysicalPath = Path.IsPathRooted(storagePath)
    ? storagePath
    : Path.Combine(app.Environment.ContentRootPath, storagePath);
Directory.CreateDirectory(uploadsPhysicalPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPhysicalPath),
    RequestPath = "/uploads"
});

app.UseCors();
app.UseRateLimiter();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health Check
app.MapGet("/api/health", async (HealthCheckService healthCheckService, CancellationToken ct) =>
{
    var report = await healthCheckService.CheckHealthAsync(ct);
    var response = new
    {
        status = report.Status.ToString(),
        totalDuration = report.TotalDuration.TotalMilliseconds,
        entries = report.Entries.ToDictionary(
            e => e.Key,
            e => new { status = e.Value.Status.ToString(), description = e.Value.Description, duration = e.Value.Duration.TotalMilliseconds })
    };
    return report.Status == HealthStatus.Healthy
        ? Results.Ok(response)
        : Results.Json(response, statusCode: 503);
})
.ExcludeFromDescription();
// En el entorno Testing, los tests manejan su propio setup de DB y roles.
// El seeder se salta para evitar que arranque antes de que las migraciones estén aplicadas.
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var db          = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var config      = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var logger      = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    await RoleSeeder.SeedAsync(roleManager, userManager, config, logger);
    await DataSeeder.SeedAsync(userManager, db, logger);
    await LanguageSeeder.SeedAsync(db, logger);
}
app.Run();
