using System.Net;
using System.Net.Http.Json;
using BlogPlatform.Application.DTOs.Auth;
using BlogPlatform.Domain.Entities.Auth;
using BlogPlatform.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace BlogPlatform.IntegrationTests.Auth;

/// <summary>
/// Pruebas de integración del flujo de autenticación.
///
/// A diferencia de los unit tests, aquí no mockeamos nada — el request HTTP recorre
/// toda la cadena: ExceptionMiddleware → routing → AuthController → AuthService →
/// FluentValidation → EF Core → PostgreSQL (DB de test) → response HTTP.
///
/// IClassFixture<BlogPlatformWebAppFactory>:
///   La factory se crea UNA vez para toda la clase. Todos los tests comparten la
///   misma instancia de la app en memoria.
///
/// IAsyncLifetime:
///   InitializeAsync() se ejecuta ANTES de cada test individual.
///   DisposeAsync() se ejecuta DESPUÉS de cada test individual.
///   Esto nos permite resetear la DB antes de cada test para que sean independientes.
/// </summary>
public class AuthFlowTests : IClassFixture<BlogPlatformWebAppFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly BlogPlatformWebAppFactory _factory;
    private readonly DatabaseHelper _dbHelper = new();

    public AuthFlowTests(BlogPlatformWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Asegurar que el schema de la DB de test existe (aplica migraciones si no existen)
        await _factory.EnsureDatabaseMigratedAsync();

        // Inicializar Respawn (analiza el grafo de FK de la DB)
        await _dbHelper.InitializeAsync();

        // Limpiar datos de tests anteriores
        await _dbHelper.ResetAsync();

        // Sembrar los roles que la app necesita para funcionar.
        // Los roles se excluyen del reset de Respawn, así que solo los creamos
        // si no existen aún.
        using var scope = _factory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        foreach (var role in new[] { "SuperAdmin", "Admin", "Blogger", "Reader" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole(role));
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // ── Register ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_WithValidData_Returns201WithAccessToken()
    {
        var request = new RegisterRequest
        {
            Email = "integration@test.com",
            Password = "Password1!",
            DisplayName = "Integration User",
            Username = "integration_user"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body.Should().NotBeNull();
        body!.Email.Should().Be("integration@test.com");
        body.AccessToken.Should().NotBeNullOrEmpty();
        // El rol por defecto en el registro es Reader (no Blogger)
        body.Roles.Should().Contain("Reader");
        body.Roles.Should().NotContain("Blogger");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409()
    {
        var first = new RegisterRequest
        {
            Email = "duplicate@test.com",
            Password = "Password1!",
            DisplayName = "First User",
            Username = "first_user"
        };
        await _client.PostAsJsonAsync("/api/auth/register", first);

        // Mismo email, diferente username
        var duplicate = new RegisterRequest
        {
            Email = "duplicate@test.com",
            Password = "Password1!",
            DisplayName = "Second User",
            Username = "second_user"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", duplicate);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_Returns409()
    {
        var first = new RegisterRequest
        {
            Email = "first@test.com",
            Password = "Password1!",
            DisplayName = "First User",
            Username = "shared_username"
        };
        await _client.PostAsJsonAsync("/api/auth/register", first);

        var duplicate = new RegisterRequest
        {
            Email = "second@test.com",
            Password = "Password1!",
            DisplayName = "Second User",
            Username = "shared_username" // mismo username
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", duplicate);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_Returns400()
    {
        // Este test verifica que el ExceptionMiddleware captura la ValidationException
        // de FluentValidation y la convierte en HTTP 400.
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = "notanemail",
            Password = "Password1!",
            DisplayName = "User",
            Username = "testuser"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithWeakPassword_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = "user@test.com",
            Password = "weak",  // sin mayúscula, sin dígito, menos de 8 chars
            DisplayName = "User",
            Username = "testuser"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
