using BlogPlatform.Domain.Entities.Auth;
using BlogPlatform.Domain.Entities.Content;
using BlogPlatform.Infrastructure.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Infrastructure.Persistence.Seed;

public static class RoleSeeder
{
    public static readonly string[] Roles = ["SuperAdmin", "Admin", "Blogger", "Reader"];

    public static async Task SeedAsync(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        AppDbContext db,
        IConfiguration configuration,
        ILogger logger)
    {
        foreach (var roleName in Roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName));
                logger.LogInformation("Role created: {Role}", roleName);
            }
        }

        var superAdminEmail = configuration["Seed:SuperAdminEmail"] ?? "superadmin@blogplatform.local";
        var superAdminPassword = configuration["Seed:SuperAdminPassword"] ?? "Admin@123456";

        var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);
        if (superAdmin is null)
        {
            superAdmin = new ApplicationUser
            {
                Email = superAdminEmail,
                UserName = "superadmin",
                DisplayName = "Super Admin",
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(superAdmin, superAdminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                logger.LogInformation("SuperAdmin user created: {Email}", superAdminEmail);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogWarning("Failed to create SuperAdmin: {Errors}", errors);
                return;
            }
        }

        await EnsureBlogProfileAsync(db, superAdmin, logger);
    }

    // Todo usuario necesita un BlogProfile: es la entidad de la que cuelgan posts,
    // preferencias y el perfil publico. El registro normal lo crea en AuthService,
    // pero el SuperAdmin se crea aqui, por lo que hay que crearlo tambien.
    private static async Task EnsureBlogProfileAsync(
        AppDbContext db,
        ApplicationUser user,
        ILogger logger)
    {
        if (await db.Set<BlogProfile>().AnyAsync(p => p.UserId == user.Id))
            return;

        var baseSlug = SlugHelper.Generate(user.UserName ?? user.Email ?? user.Id.ToString());
        var existingSlugs = await db.Set<BlogProfile>()
            .Where(p => p.Slug.StartsWith(baseSlug))
            .Select(p => p.Slug)
            .ToListAsync();

        db.Set<BlogProfile>().Add(new BlogProfile
        {
            UserId = user.Id,
            Slug = SlugHelper.MakeUnique(baseSlug, existingSlugs)
        });
        await db.SaveChangesAsync();

        logger.LogInformation("BlogProfile created for {UserName}", user.UserName);
    }
}
