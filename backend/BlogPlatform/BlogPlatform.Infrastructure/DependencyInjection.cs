using BlogPlatform.Application.DTOs.Admin;
using BlogPlatform.Application.DTOs.Auth;
using BlogPlatform.Application.DTOs.Community;
using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Application.DTOs.Profile;
using BlogPlatform.Application.Interfaces;
using BlogPlatform.Application.Validators.Auth;
using BlogPlatform.Application.Validators.Community;
using BlogPlatform.Application.Validators.Posts;
using BlogPlatform.Application.Validators.Profile;
using BlogPlatform.Domain.Entities.Auth;
using BlogPlatform.Domain.Interfaces;
using BlogPlatform.Infrastructure.Persistence;
using BlogPlatform.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace BlogPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("Default"))
                .UseSnakeCaseNamingConvention());

        services.AddIdentity<ApplicationUser, ApplicationRole>(opts =>
        {
            opts.Lockout.MaxFailedAccessAttempts = 5;
            opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            opts.User.RequireUniqueEmail = true;
            opts.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        services.AddHttpContextAccessor();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IStorageService, LocalStorageService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IFollowService, FollowService>();
        services.AddScoped<IBlockService, BlockService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFeedService, FeedService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IBlogListService, BlogListService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<INewsletterService, NewsletterService>();
        services.AddScoped<IPlatformSettingsService, PlatformSettingsService>();
        services.AddScoped<ILanguageService, LanguageService>();

        var emailProvider = configuration["Email:Provider"];
        if (string.Equals(emailProvider, "SendGrid", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IEmailService, SendGridEmailService>();
        }
        else
        {
            services.AddScoped<IEmailService, ConsoleEmailService>();
        }

        // Redis: registro condicional — si no hay Redis disponible (config vacía o
        // servidor inalcanzable al arrancar), se usa in-memory como fallback.
        var redisConnectionString = configuration["Redis:ConnectionString"];
        IConnectionMultiplexer? redis = null;
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            try
            {
                var options = ConfigurationOptions.Parse(redisConnectionString);
                options.AbortOnConnectFail = false;
                redis = ConnectionMultiplexer.Connect(options);
                if (!redis.IsConnected)
                    redis = null;
            }
            catch (Exception)
            {
                redis = null;
            }
        }

        if (redis is not null)
        {
            services.AddSingleton(redis);
            services.AddStackExchangeRedisCache(opts =>
            {
                opts.Configuration = redisConnectionString;
                opts.InstanceName = "blogplatform:";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddHostedService<ScheduledPublishingService>();

        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<ForgotPasswordRequest>, ForgotPasswordRequestValidator>();
        services.AddScoped<IValidator<ResetPasswordRequest>, ResetPasswordRequestValidator>();
        services.AddScoped<IValidator<CreatePostRequest>, CreatePostRequestValidator>();
        services.AddScoped<IValidator<UpdatePostRequest>, UpdatePostRequestValidator>();
        services.AddScoped<IValidator<UpdateProfileRequest>, UpdateProfileRequestValidator>();
        services.AddScoped<IValidator<CreateCommentRequest>, CreateCommentRequestValidator>();
        services.AddScoped<IValidator<UpdateCommentRequest>, UpdateCommentRequestValidator>();

        return services;
    }
}
