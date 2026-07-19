using BlogPlatform.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Infrastructure.Persistence.Seed;

public static class RoleSeeder
{
    public static readonly string[] Roles = ["SuperAdmin", "Admin", "Blogger", "Reader"];

    public static async Task SeedAsync(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
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

        var existing = await userManager.FindByEmailAsync(superAdminEmail);
        if (existing is null)
        {
            var superAdmin = new ApplicationUser
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
            }
        }
    }
}
