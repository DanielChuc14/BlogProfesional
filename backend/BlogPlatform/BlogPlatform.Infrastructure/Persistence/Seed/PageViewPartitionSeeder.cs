using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Infrastructure.Persistence.Seed;

// page_views esta particionada por RANGE(created_at). La migracion inicial solo
// creo particiones hasta 2026-05, asi que a partir de esa fecha cualquier INSERT
// falla con "no partition of relation page_views found for row" y se pierden todas
// las visitas. Crear particiones en cada arranque evita que vuelvan a caducar.
public static class PageViewPartitionSeeder
{
    public static async Task EnsureAsync(AppDbContext db, ILogger logger, int monthsAhead = 3)
    {
        var now = DateTime.UtcNow;
        var firstOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        for (var i = 0; i <= monthsAhead; i++)
        {
            var from = firstOfMonth.AddMonths(i);
            var to = from.AddMonths(1);

            // El nombre y las fechas derivan del calendario, no de entrada externa.
            var partition = $"page_views_y{from:yyyy}m{from:MM}";
            var sql = $"""
                CREATE TABLE IF NOT EXISTS {partition}
                    PARTITION OF page_views
                    FOR VALUES FROM ('{from:yyyy-MM-dd}') TO ('{to:yyyy-MM-dd}');
                """;

            await db.Database.ExecuteSqlRawAsync(sql);
        }

        logger.LogInformation(
            "PageView partitions ensured through {Through:yyyy-MM}",
            firstOfMonth.AddMonths(monthsAhead));
    }
}
