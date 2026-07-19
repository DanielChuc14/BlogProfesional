using BlogPlatform.Domain.Entities.Auth;
using BlogPlatform.Domain.Entities.Content;
using BlogPlatform.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Infrastructure.Persistence.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        AppDbContext db,
        ILogger logger)
    {
        // Evita re-ejecutar si ya hay datos más allá del superadmin
        if (await db.Tags.AnyAsync())
        {
            logger.LogInformation("DataSeeder: datos de prueba ya existentes, se omite.");
            return;
        }

        logger.LogInformation("DataSeeder: iniciando seed de datos de prueba...");

        // ── Usuarios ─────────────────────────────────────────────────────────

        var admin = await CreateUserAsync(userManager, logger,
            email: "admin@blogplatform.local",
            userName: "admin",
            displayName: "Platform Admin",
            password: "Admin@123456",
            role: "Admin");

        var blogger1 = await CreateUserAsync(userManager, logger,
            email: "alice@blogplatform.local",
            userName: "alice",
            displayName: "Alice Dev",
            bio: "Full-stack developer. I write about .NET, Angular and clean architecture.",
            password: "Blogger@123456",
            role: "Blogger");

        var blogger2 = await CreateUserAsync(userManager, logger,
            email: "bob@blogplatform.local",
            userName: "bob",
            displayName: "Bob UX",
            bio: "UX/UI designer turned frontend developer. CSS fanatic.",
            password: "Blogger@123456",
            role: "Blogger");

        var reader1 = await CreateUserAsync(userManager, logger,
            email: "reader1@blogplatform.local",
            userName: "reader1",
            displayName: "Carlos R.",
            password: "Reader@123456",
            role: "Reader");

        var reader2 = await CreateUserAsync(userManager, logger,
            email: "reader2@blogplatform.local",
            userName: "reader2",
            displayName: "Diana M.",
            password: "Reader@123456",
            role: "Reader");

        // ── BlogProfiles ─────────────────────────────────────────────────────

        var profileAdmin = await CreateProfileAsync(db, admin!, "admin");
        var profileAlice = await CreateProfileAsync(db, blogger1!, "alice",
            about: "Full-stack developer. I write about .NET, Angular and clean architecture.");
        var profileBob = await CreateProfileAsync(db, blogger2!, "bob",
            about: "UX/UI designer turned frontend developer. CSS fanatic.");

        await db.SaveChangesAsync();

        // ── Tags ──────────────────────────────────────────────────────────────

        var tags = new List<Tag>
        {
            new() { Name = ".NET",          Slug = "dotnet" },
            new() { Name = "Angular",       Slug = "angular" },
            new() { Name = "C#",            Slug = "csharp" },
            new() { Name = "Clean Architecture", Slug = "clean-architecture" },
            new() { Name = "CSS",           Slug = "css" },
            new() { Name = "Docker",        Slug = "docker" },
            new() { Name = "Entity Framework", Slug = "entity-framework" },
            new() { Name = "JavaScript",    Slug = "javascript" },
            new() { Name = "PostgreSQL",    Slug = "postgresql" },
            new() { Name = "REST API",      Slug = "rest-api" },
            new() { Name = "Tailwind CSS",  Slug = "tailwind-css" },
            new() { Name = "TypeScript",    Slug = "typescript" },
            new() { Name = "UX Design",     Slug = "ux-design" },
            new() { Name = "Web Performance", Slug = "web-performance" },
            new() { Name = "Beginner",      Slug = "beginner" },
        };

        await db.Tags.AddRangeAsync(tags);
        await db.SaveChangesAsync();

        var tagMap = tags.ToDictionary(t => t.Slug);

        // ── Posts de Alice ────────────────────────────────────────────────────

        await CreatePostAsync(db, profileAlice!,
            title: "Getting started with Clean Architecture in .NET",
            slug: "getting-started-clean-architecture-dotnet",
            excerpt: "A practical introduction to structuring .NET solutions using Clean Architecture principles.",
            content: """
                # Getting started with Clean Architecture in .NET

                Clean Architecture separates your application into concentric layers where dependencies
                only point inward. The core idea: business rules should not depend on frameworks,
                databases, or external services.

                ## The four layers

                **Domain** — entities, value objects, enums, interfaces. No dependencies on anything else.

                **Application** — use cases, DTOs, service interfaces. Depends only on Domain.

                **Infrastructure** — database, email, storage implementations. Depends on Application.

                **API / Presentation** — controllers, middleware. Orchestrates everything.

                ## Why bother?

                - Business logic is testable without spinning up a database
                - You can swap EF Core for Dapper or even MongoDB without touching your domain
                - New team members can understand the structure at a glance

                ## Setting up the solution

                ```bash
                dotnet new sln -o MyApp
                dotnet new classlib -o MyApp/src/MyApp.Domain
                dotnet new classlib -o MyApp/src/MyApp.Application
                dotnet new classlib -o MyApp/src/MyApp.Infrastructure
                dotnet new webapi   -o MyApp/src/MyApp.API
                ```

                Add project references so dependencies only flow inward and you are done with the scaffold.
                """,
            status: PostStatus.Published,
            publishedAt: DateTime.UtcNow.AddDays(-10),
            readTime: 6,
            tags: [tagMap["dotnet"], tagMap["clean-architecture"], tagMap["csharp"]]);

        await CreatePostAsync(db, profileAlice!,
            title: "Entity Framework Core: tips you might not know",
            slug: "entity-framework-core-tips",
            excerpt: "Lesser-known EF Core features that can save you hours of debugging.",
            content: """
                # Entity Framework Core: tips you might not know

                After working with EF Core on several production projects, here are the features
                I wish I had known from day one.

                ## 1. UseSnakeCaseNamingConvention

                Using PostgreSQL? Add `EFCore.NamingConventions` and call `.UseSnakeCaseNamingConvention()`
                on your DbContextOptionsBuilder. Your columns will automatically map to `snake_case`
                without manual `[Column]` attributes everywhere.

                ## 2. Compiled queries

                For hot paths that run thousands of times per second, compiled queries skip the
                expression-tree translation overhead:

                ```csharp
                private static readonly Func<AppDbContext, Guid, Task<Post?>> GetPostById =
                    EF.CompileAsyncQuery((AppDbContext db, Guid id) =>
                        db.Posts.FirstOrDefault(p => p.Id == id));
                ```

                ## 3. ExecuteUpdateAsync / ExecuteDeleteAsync

                Stop loading entities just to delete or update them in bulk:

                ```csharp
                await db.Posts
                    .Where(p => p.Status == PostStatus.Archived)
                    .ExecuteDeleteAsync();
                ```

                No change tracking, no in-memory objects — pure SQL.
                """,
            status: PostStatus.Published,
            publishedAt: DateTime.UtcNow.AddDays(-5),
            readTime: 5,
            tags: [tagMap["dotnet"], tagMap["entity-framework"], tagMap["postgresql"]]);

        await CreatePostAsync(db, profileAlice!,
            title: "Building a REST API with ASP.NET Core 10",
            slug: "rest-api-aspnet-core-10",
            excerpt: "Step-by-step guide to building a production-ready REST API.",
            content: "Draft content — work in progress.",
            status: PostStatus.Draft,
            readTime: 8,
            tags: [tagMap["dotnet"], tagMap["rest-api"], tagMap["csharp"]]);

        // ── Posts de Bob ──────────────────────────────────────────────────────

        await CreatePostAsync(db, profileBob!,
            title: "Tailwind CSS: utility-first is not a dirty word",
            slug: "tailwind-css-utility-first",
            excerpt: "Why Tailwind CSS clicked for me after years of BEM and SCSS.",
            content: """
                # Tailwind CSS: utility-first is not a dirty word

                I spent years building design systems with BEM, SCSS, and CSS Modules.
                When Tailwind first appeared I dismissed it as "inline styles with extra steps."
                I was wrong.

                ## What changed my mind

                The key insight is that utility classes are **not** the same as inline styles.
                They are constrained by a design system — spacing scale, color palette, type scale —
                enforced at the class level, not at the individual element level.

                ## The productivity argument

                With a traditional approach you constantly context-switch between HTML and CSS files.
                With Tailwind your design decisions live right next to your markup:

                ```html
                <button class="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition">
                  Submit
                </button>
                ```

                No naming, no specificity wars, no dead CSS.

                ## When it falls short

                Tailwind shines for component-level UI. For complex animations or highly custom
                one-off designs, a thin layer of custom CSS alongside Tailwind is perfectly fine.
                """,
            status: PostStatus.Published,
            publishedAt: DateTime.UtcNow.AddDays(-7),
            readTime: 4,
            tags: [tagMap["css"], tagMap["tailwind-css"], tagMap["javascript"]]);

        await CreatePostAsync(db, profileBob!,
            title: "TypeScript for Angular developers: what you actually need",
            slug: "typescript-angular-developers",
            excerpt: "Cut through the TypeScript noise and focus on the features that matter for Angular.",
            content: """
                # TypeScript for Angular developers: what you actually need

                Angular is TypeScript-first, but you don't need to master every feature of the language
                to be productive. Here is what I use daily.

                ## Generics

                Most useful for service methods and HTTP calls:

                ```typescript
                get<T>(path: string): Observable<T> {
                  return this.http.get<T>(`${this.base}${path}`);
                }
                ```

                ## Discriminated unions

                Perfect for modelling loading states:

                ```typescript
                type LoadState<T> =
                  | { status: 'idle' }
                  | { status: 'loading' }
                  | { status: 'success'; data: T }
                  | { status: 'error'; message: string };
                ```

                ## Signals (Angular 17+)

                Not strictly TypeScript but worth mentioning: `signal<T>()`, `computed()`, and
                `effect()` replace most RxJS in component state. Simpler mental model, better
                change detection performance.
                """,
            status: PostStatus.Published,
            publishedAt: DateTime.UtcNow.AddDays(-3),
            readTime: 5,
            tags: [tagMap["typescript"], tagMap["angular"], tagMap["javascript"]]);

        await CreatePostAsync(db, profileBob!,
            title: "Web performance checklist 2026",
            slug: "web-performance-checklist-2026",
            excerpt: "The checklist I run through before every production release.",
            content: "Draft — gathering notes.",
            status: PostStatus.Draft,
            readTime: 7,
            tags: [tagMap["web-performance"], tagMap["javascript"]]);

        await db.SaveChangesAsync();

        logger.LogInformation("DataSeeder: seed completado.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static async Task<ApplicationUser?> CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        string email,
        string userName,
        string displayName,
        string password,
        string role,
        string? bio = null)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
            return await userManager.FindByEmailAsync(email);

        var user = new ApplicationUser
        {
            Email = email,
            UserName = userName,
            DisplayName = displayName,
            Bio = bio,
            EmailConfirmed = true,
            IsActive = true,
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            logger.LogWarning("DataSeeder: no se pudo crear {Email} — {Errors}",
                email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return null;
        }

        await userManager.AddToRoleAsync(user, role);
        logger.LogInformation("DataSeeder: usuario creado {Email} [{Role}]", email, role);
        return user;
    }

    private static async Task<BlogProfile?> CreateProfileAsync(
        AppDbContext db,
        ApplicationUser user,
        string slug,
        string? about = null)
    {
        if (await db.Set<BlogProfile>().AnyAsync(p => p.UserId == user.Id))
            return await db.Set<BlogProfile>().FirstAsync(p => p.UserId == user.Id);

        var profile = new BlogProfile
        {
            UserId = user.Id,
            Slug = slug,
            About = about,
        };
        await db.Set<BlogProfile>().AddAsync(profile);
        return profile;
    }

    private static async Task CreatePostAsync(
        AppDbContext db,
        BlogProfile profile,
        string title,
        string slug,
        string excerpt,
        string content,
        PostStatus status,
        int readTime,
        List<Tag> tags,
        DateTime? publishedAt = null)
    {
        if (await db.Set<Post>().AnyAsync(p => p.Slug == slug))
            return;

        var post = new Post
        {
            BlogProfileId = profile.Id,
            Title = title,
            Slug = slug,
            Excerpt = excerpt,
            Content = content,
            Status = status,
            PublishedAt = publishedAt,
            ReadTimeMinutes = readTime,
        };

        await db.Set<Post>().AddAsync(post);
        await db.SaveChangesAsync();

        foreach (var tag in tags)
        {
            db.Set<PostTag>().Add(new PostTag { PostId = post.Id, TagId = tag.Id });
            tag.PostCount++;
        }
    }
}
