using BlogPlatform.Domain.Entities.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Infrastructure.Persistence.Seed;

public static class LanguageSeeder
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger)
    {
        if (await db.Languages.AnyAsync())
        {
            logger.LogInformation("LanguageSeeder: idiomas ya existentes, se omite.");
            return;
        }

        logger.LogInformation("LanguageSeeder: creando idiomas predeterminados...");

        var languages = new List<Language>
        {
            new()
            {
                Code = "en",
                Name = "English",
                NativeName = "English",
                IsActive = true,
                IsDefault = true,
                TranslationJson = null,
            },
            new()
            {
                Code = "es",
                Name = "Spanish",
                NativeName = "Español",
                IsActive = true,
                IsDefault = false,
                TranslationJson = null,
            },
        };

        await db.Languages.AddRangeAsync(languages);
        await db.SaveChangesAsync();

        logger.LogInformation("LanguageSeeder: creados {Count} idiomas.", languages.Count);
    }
}
