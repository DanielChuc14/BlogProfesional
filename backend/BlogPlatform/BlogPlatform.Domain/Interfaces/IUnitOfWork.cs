using BlogPlatform.Domain.Entities.Admin;
using BlogPlatform.Domain.Entities.Analytics;
using BlogPlatform.Domain.Entities.Auth;
using BlogPlatform.Domain.Entities.Community;
using BlogPlatform.Domain.Entities.Content;
using BlogPlatform.Domain.Entities.Monetization;
using BlogPlatform.Domain.Entities.Security;

namespace BlogPlatform.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<RefreshToken> RefreshTokens { get; }
    IRepository<BlogProfile> BlogProfiles { get; }
    IRepository<BlogTheme> BlogThemes { get; }
    IRepository<SocialLink> SocialLinks { get; }
    IRepository<Post> Posts { get; }
    IRepository<SlugRedirect> SlugRedirects { get; }
    IRepository<Tag> Tags { get; }
    IRepository<PostTag> PostTags { get; }
    IRepository<PostSeo> PostSeos { get; }
    IRepository<PostMedia> PostMediaItems { get; }
    IRepository<Comment> Comments { get; }
    IRepository<CommentLike> CommentLikes { get; }
    IRepository<PostLike> PostLikes { get; }
    IRepository<Follow> Follows { get; }
    IRepository<Notification> Notifications { get; }
    IRepository<PageView> PageViews { get; }
    IRepository<DailyStat> DailyStats { get; }
    IRepository<UserWordFilter> UserWordFilters { get; }
    IRepository<BlogNotice> BlogNotices { get; }
    IRepository<QuickLink> QuickLinks { get; }
    IRepository<BlogList> BlogLists { get; }
    IRepository<BlogListItem> BlogListItems { get; }

    // Community
    IRepository<NewsletterSend> NewsletterSends { get; }

    // Security
    IRepository<UserBlock> UserBlocks { get; }
    IRepository<Report> Reports { get; }
    IRepository<UserSuspension> UserSuspensions { get; }

    // Admin
    IRepository<AuditLog> AuditLogs { get; }
    IRepository<RestrictedWord> RestrictedWords { get; }
    IRepository<Language> Languages { get; }

    // Monetization
    IRepository<StripeAccount> StripeAccounts { get; }
    IRepository<BlogPlan> BlogPlans { get; }
    IRepository<Subscription> Subscriptions { get; }
    IRepository<Payment> Payments { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IRepository<T> where T : class
{
    IQueryable<T> Query();
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<T?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}
