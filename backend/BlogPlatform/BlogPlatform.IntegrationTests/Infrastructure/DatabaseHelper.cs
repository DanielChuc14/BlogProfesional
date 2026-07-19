using Npgsql;
using Respawn;

namespace BlogPlatform.IntegrationTests.Infrastructure;

/// <summary>
/// Maneja el reset de la base de datos entre tests usando Respawn.
///
/// ¿Por qué Respawn y no DELETE manual?
/// Cuando tienes foreign keys (por ejemplo refresh_tokens.user_id → asp_net_users.id),
/// hacer DELETE FROM asp_net_users antes de borrar los refresh tokens falla con un error
/// de constraint. Respawn calcula automáticamente el orden correcto de borrado
/// basándose en el grafo de dependencias de la DB — sin necesidad de que tú lo sepas.
/// Es también más rápido que truncar tablas individualmente.
/// </summary>
public class DatabaseHelper
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=blogplatform_test;Username=postgres;Password=postgresql";

    private Respawner? _respawner;

    public async Task InitializeAsync()
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();

        _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore =
            [
                // Los roles se siembran una sola vez antes de los tests.
                new Respawn.Graph.Table("asp_net_roles"),
                // El historial de migraciones de EF Core no es datos de test.
                // Si Respawn lo vacía, MigrateAsync() intenta re-crear tablas que ya existen.
                new Respawn.Graph.Table("__EFMigrationsHistory")
            ]
        });
    }

    public async Task ResetAsync()
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        await _respawner!.ResetAsync(conn);
    }
}
