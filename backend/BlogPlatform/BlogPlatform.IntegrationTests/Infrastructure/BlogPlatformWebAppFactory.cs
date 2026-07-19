using BlogPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlogPlatform.IntegrationTests.Infrastructure;

/// <summary>
/// WebApplicationFactory levanta la aplicación completa en memoria para los tests.
///
/// No abre puertos reales ni necesita IIS. Puedes hacer requests HTTP contra tu API
/// completa incluyendo middleware, routing, autenticación y EF Core, pero usando
/// la base de datos de test en lugar de la de desarrollo.
///
/// IClassFixture<BlogPlatformWebAppFactory> en la clase de test significa que esta
/// factory se crea UNA sola vez para toda la clase — los tests comparten la misma
/// instancia de la app. La DB se limpia entre tests individualmente con Respawn.
/// </summary>
public class BlogPlatformWebAppFactory : WebApplicationFactory<Program>
{
    private const string TestConnectionString =
        "Host=localhost;Port=5432;Database=blogplatform_test;Username=postgres;Password=postgresql";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Indicar que estamos en el entorno "Testing"
        builder.UseEnvironment("Testing");

        // Cargar la configuración específica de tests
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile("appsettings.Testing.json", optional: false);
        });

        builder.ConfigureServices(services =>
        {
            // Eliminar el DbContext registrado por la app (que apunta a la DB de desarrollo)
            // y reemplazarlo con uno que apunta a la DB de test.
            services.RemoveAll<DbContextOptions<AppDbContext>>();

            services.AddDbContext<AppDbContext>(opts =>
                opts.UseNpgsql(TestConnectionString)
                    .UseSnakeCaseNamingConvention());
        });
    }

    /// <summary>
    /// Aplica las migraciones de EF Core a la base de datos de test.
    /// Usa un DbContext standalone en lugar de Services.CreateScope() para evitar
    /// problemas con el ciclo de vida del service provider de la factory.
    /// Es idempotente: si las migraciones ya están aplicadas, no hace nada.
    /// </summary>
    public async Task EnsureDatabaseMigratedAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(TestConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using var db = new AppDbContext(options);
        await db.Database.MigrateAsync();
    }
}
