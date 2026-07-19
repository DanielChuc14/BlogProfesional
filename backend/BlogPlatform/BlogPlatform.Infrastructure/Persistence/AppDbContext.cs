using BlogPlatform.Domain.Entities.Admin;
using BlogPlatform.Domain.Entities.Analytics;
using BlogPlatform.Domain.Entities.Auth;
using BlogPlatform.Domain.Entities.Community;
using BlogPlatform.Domain.Entities.Content;
using BlogPlatform.Domain.Entities.Monetization;
using BlogPlatform.Domain.Entities.Security;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<BlogProfile> BlogProfiles => Set<BlogProfile>();
    public DbSet<BlogTheme> BlogThemes => Set<BlogTheme>();
    public DbSet<SocialLink> SocialLinks => Set<SocialLink>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<SlugRedirect> SlugRedirects => Set<SlugRedirect>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    public DbSet<PostSeo> PostSeos => Set<PostSeo>();
    public DbSet<PostMedia> PostMediaItems => Set<PostMedia>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<CommentLike> CommentLikes => Set<CommentLike>();
    public DbSet<PostLike> PostLikes => Set<PostLike>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<PageView> PageViews => Set<PageView>();
    public DbSet<DailyStat> DailyStats => Set<DailyStat>();
    public DbSet<UserWordFilter> UserWordFilters => Set<UserWordFilter>();
    public DbSet<BlogNotice> BlogNotices => Set<BlogNotice>();
    public DbSet<QuickLink> QuickLinks => Set<QuickLink>();
    public DbSet<BlogList> BlogLists => Set<BlogList>();
    public DbSet<BlogListItem> BlogListItems => Set<BlogListItem>();

    // Community
    public DbSet<NewsletterSend> NewsletterSends => Set<NewsletterSend>();

    // Security
    public DbSet<UserBlock> UserBlocks => Set<UserBlock>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<UserSuspension> UserSuspensions => Set<UserSuspension>();

    // Admin
    public DbSet<PlatformSetting> PlatformSettings => Set<PlatformSetting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RestrictedWord> RestrictedWords => Set<RestrictedWord>();
    public DbSet<Language> Languages => Set<Language>();

    // Monetization (inactive — requires is_monetization_enabled = true)
    public DbSet<StripeAccount> StripeAccounts => Set<StripeAccount>();
    public DbSet<BlogPlan> BlogPlans => Set<BlogPlan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
