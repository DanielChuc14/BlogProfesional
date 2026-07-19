using BlogPlatform.Domain.Entities.Admin;
using BlogPlatform.Domain.Entities.Analytics;
using BlogPlatform.Domain.Entities.Auth;
using BlogPlatform.Domain.Entities.Community;
using BlogPlatform.Domain.Entities.Content;
using BlogPlatform.Domain.Entities.Monetization;
using BlogPlatform.Domain.Entities.Security;
using BlogPlatform.Domain.Interfaces;

namespace BlogPlatform.Infrastructure.Persistence;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private IRepository<RefreshToken>? _refreshTokens;
    private IRepository<BlogProfile>? _blogProfiles;
    private IRepository<BlogTheme>? _blogThemes;
    private IRepository<SocialLink>? _socialLinks;
    private IRepository<Post>? _posts;
    private IRepository<SlugRedirect>? _slugRedirects;
    private IRepository<Tag>? _tags;
    private IRepository<PostTag>? _postTags;
    private IRepository<PostSeo>? _postSeos;
    private IRepository<PostMedia>? _postMediaItems;
    private IRepository<Comment>? _comments;
    private IRepository<CommentLike>? _commentLikes;
    private IRepository<PostLike>? _postLikes;
    private IRepository<Follow>? _follows;
    private IRepository<Notification>? _notifications;
    private IRepository<PageView>? _pageViews;
    private IRepository<DailyStat>? _dailyStats;
    private IRepository<UserWordFilter>? _userWordFilters;
    private IRepository<BlogNotice>? _blogNotices;
    private IRepository<QuickLink>? _quickLinks;
    private IRepository<BlogList>? _blogLists;
    private IRepository<BlogListItem>? _blogListItems;
    private IRepository<NewsletterSend>? _newsletterSends;
    private IRepository<UserBlock>? _userBlocks;
    private IRepository<Report>? _reports;
    private IRepository<UserSuspension>? _userSuspensions;
    private IRepository<AuditLog>? _auditLogs;
    private IRepository<RestrictedWord>? _restrictedWords;
    private IRepository<Language>? _languages;
    private IRepository<StripeAccount>? _stripeAccounts;
    private IRepository<BlogPlan>? _blogPlans;
    private IRepository<Subscription>? _subscriptions;
    private IRepository<Payment>? _payments;

    public IRepository<RefreshToken> RefreshTokens =>
        _refreshTokens ??= new Repository<RefreshToken>(context);

    public IRepository<BlogProfile> BlogProfiles =>
        _blogProfiles ??= new Repository<BlogProfile>(context);

    public IRepository<BlogTheme> BlogThemes =>
        _blogThemes ??= new Repository<BlogTheme>(context);

    public IRepository<SocialLink> SocialLinks =>
        _socialLinks ??= new Repository<SocialLink>(context);

    public IRepository<Post> Posts =>
        _posts ??= new Repository<Post>(context);

    public IRepository<SlugRedirect> SlugRedirects =>
        _slugRedirects ??= new Repository<SlugRedirect>(context);

    public IRepository<Tag> Tags =>
        _tags ??= new Repository<Tag>(context);

    public IRepository<PostTag> PostTags =>
        _postTags ??= new Repository<PostTag>(context);

    public IRepository<PostSeo> PostSeos =>
        _postSeos ??= new Repository<PostSeo>(context);

    public IRepository<PostMedia> PostMediaItems =>
        _postMediaItems ??= new Repository<PostMedia>(context);

    public IRepository<Comment> Comments =>
        _comments ??= new Repository<Comment>(context);

    public IRepository<CommentLike> CommentLikes =>
        _commentLikes ??= new Repository<CommentLike>(context);

    public IRepository<PostLike> PostLikes =>
        _postLikes ??= new Repository<PostLike>(context);

    public IRepository<Follow> Follows =>
        _follows ??= new Repository<Follow>(context);

    public IRepository<Notification> Notifications =>
        _notifications ??= new Repository<Notification>(context);

    public IRepository<PageView> PageViews =>
        _pageViews ??= new Repository<PageView>(context);

    public IRepository<DailyStat> DailyStats =>
        _dailyStats ??= new Repository<DailyStat>(context);

    public IRepository<UserWordFilter> UserWordFilters =>
        _userWordFilters ??= new Repository<UserWordFilter>(context);

    public IRepository<BlogNotice> BlogNotices =>
        _blogNotices ??= new Repository<BlogNotice>(context);

    public IRepository<QuickLink> QuickLinks =>
        _quickLinks ??= new Repository<QuickLink>(context);

    public IRepository<BlogList> BlogLists =>
        _blogLists ??= new Repository<BlogList>(context);

    public IRepository<BlogListItem> BlogListItems =>
        _blogListItems ??= new Repository<BlogListItem>(context);

    public IRepository<NewsletterSend> NewsletterSends =>
        _newsletterSends ??= new Repository<NewsletterSend>(context);

    public IRepository<UserBlock> UserBlocks =>
        _userBlocks ??= new Repository<UserBlock>(context);

    public IRepository<Report> Reports =>
        _reports ??= new Repository<Report>(context);

    public IRepository<UserSuspension> UserSuspensions =>
        _userSuspensions ??= new Repository<UserSuspension>(context);

    public IRepository<AuditLog> AuditLogs =>
        _auditLogs ??= new Repository<AuditLog>(context);

    public IRepository<RestrictedWord> RestrictedWords =>
        _restrictedWords ??= new Repository<RestrictedWord>(context);

    public IRepository<Language> Languages =>
        _languages ??= new Repository<Language>(context);

    public IRepository<StripeAccount> StripeAccounts =>
        _stripeAccounts ??= new Repository<StripeAccount>(context);

    public IRepository<BlogPlan> BlogPlans =>
        _blogPlans ??= new Repository<BlogPlan>(context);

    public IRepository<Subscription> Subscriptions =>
        _subscriptions ??= new Repository<Subscription>(context);

    public IRepository<Payment> Payments =>
        _payments ??= new Repository<Payment>(context);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);

    public void Dispose() => context.Dispose();
}
