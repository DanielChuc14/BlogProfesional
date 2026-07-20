using BlogPlatform.Application.DTOs.Media;
using BlogPlatform.Application.DTOs.Profile;
using BlogPlatform.Application.Interfaces;
using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Entities.Content;
using BlogPlatform.Domain.Entities.Security;
using BlogPlatform.Domain.Enums;
using BlogPlatform.Domain.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Infrastructure.Services;

public class ProfileService(
    IUnitOfWork uow,
    IStorageService storage,
    IConfiguration configuration,
    IValidator<UpdateProfileRequest> validator,
    ILogger<ProfileService> logger) : IProfileService
{
    public async Task<ResultModel<UserProfileResponse>> GetByUsernameAsync(string username, Guid? viewerUserId = null, CancellationToken ct = default)
    {
        var profile = await uow.BlogProfiles.Query()
            .Include(p => p.User)
            .Include(p => p.SocialLinks)
            .Include(p => p.Theme)
            .Include(p => p.BlogNotices)
            .Include(p => p.QuickLinks)
            .FirstOrDefaultAsync(p => p.User.UserName == username, ct);

        if (profile is null)
            return ResultModel<UserProfileResponse>.NotFound("User not found.");

        var stats = await GetProfileStatsAsync(profile.Id, profile.UserId, viewerUserId, ct);
        return ResultModel<UserProfileResponse>.Ok(MapToResponse(profile, stats));
    }

    public async Task<ResultModel<UserProfileResponse>> UpdateAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var profile = await uow.BlogProfiles.Query()
            .Include(p => p.User)
            .Include(p => p.SocialLinks)
            .Include(p => p.Theme)
            .Include(p => p.BlogNotices)
            .Include(p => p.QuickLinks)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
            return ResultModel<UserProfileResponse>.NotFound("Profile not found.");

        if (request.DisplayName is not null)
            profile.User.DisplayName = request.DisplayName;

        if (request.Bio is not null)
            profile.User.Bio = request.Bio;

        if (request.About is not null)
            profile.About = request.About;

        if (request.LogoUrl is not null) profile.LogoUrl = request.LogoUrl;
        if (request.BannerUrl is not null) profile.BannerUrl = request.BannerUrl;
        if (request.Tagline is not null) profile.Tagline = request.Tagline;

        foreach (var link in profile.SocialLinks.ToList())
            uow.SocialLinks.Remove(link);

        foreach (var linkReq in request.SocialLinks)
        {
            await uow.SocialLinks.AddAsync(new SocialLink
            {
                BlogProfileId = profile.Id,
                Platform = linkReq.Platform,
                Url = linkReq.Url
            }, ct);
        }

        uow.BlogProfiles.Update(profile);
        await uow.SaveChangesAsync(ct);

        var updated = await uow.BlogProfiles.Query()
            .Include(p => p.User)
            .Include(p => p.SocialLinks)
            .Include(p => p.Theme)
            .Include(p => p.BlogNotices)
            .Include(p => p.QuickLinks)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        var stats = await GetProfileStatsAsync(updated!.Id, updated!.UserId, null, ct);
        return ResultModel<UserProfileResponse>.Ok(MapToResponse(updated!, stats));
    }

    public async Task<ResultModel<UserProfileResponse>> UpdateAvatarAsync(Guid userId, FileUploadDto file, CancellationToken ct = default)
    {
        var maxMb = int.TryParse(configuration["Storage:MaxImageMB"], out var mb) ? mb : 5;
        if (file.Size > maxMb * 1024L * 1024L)
            return ResultModel<UserProfileResponse>.BadRequest($"Image must not exceed {maxMb} MB.");

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return ResultModel<UserProfileResponse>.BadRequest("Only JPEG, PNG, WebP, and GIF images are allowed.");

        var profile = await uow.BlogProfiles.Query()
            .Include(p => p.User)
            .Include(p => p.SocialLinks)
            .Include(p => p.Theme)
            .Include(p => p.BlogNotices)
            .Include(p => p.QuickLinks)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
            return ResultModel<UserProfileResponse>.NotFound("Profile not found.");

        var url = await storage.SaveAsync(file.Stream, file.FileName, file.ContentType, ct);
        profile.User.AvatarUrl = url;

        uow.BlogProfiles.Update(profile);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Avatar updated for user {UserId}", userId);

        var updated = await uow.BlogProfiles.Query()
            .Include(p => p.User)
            .Include(p => p.SocialLinks)
            .Include(p => p.Theme)
            .Include(p => p.BlogNotices)
            .Include(p => p.QuickLinks)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        var stats = await GetProfileStatsAsync(updated!.Id, updated!.UserId, null, ct);
        return ResultModel<UserProfileResponse>.Ok(MapToResponse(updated!, stats));
    }

    public async Task<ResultModel<UserProfileResponse>> UpdateBannerAsync(Guid userId, FileUploadDto file, CancellationToken ct = default)
    {
        var maxMb = int.TryParse(configuration["Storage:MaxImageMB"], out var mb) ? mb : 5;
        if (file.Size > maxMb * 1024L * 1024L)
            return ResultModel<UserProfileResponse>.BadRequest($"Image must not exceed {maxMb} MB.");

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return ResultModel<UserProfileResponse>.BadRequest("Only JPEG, PNG, WebP, and GIF images are allowed.");

        var profile = await uow.BlogProfiles.Query()
            .Include(p => p.User)
            .Include(p => p.SocialLinks)
            .Include(p => p.Theme)
            .Include(p => p.BlogNotices)
            .Include(p => p.QuickLinks)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
            return ResultModel<UserProfileResponse>.NotFound("Profile not found.");

        var url = await storage.SaveAsync(file.Stream, file.FileName, file.ContentType, ct);
        profile.BannerUrl = url;

        uow.BlogProfiles.Update(profile);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Banner updated for user {UserId}", userId);

        var updated = await uow.BlogProfiles.Query()
            .Include(p => p.User)
            .Include(p => p.SocialLinks)
            .Include(p => p.Theme)
            .Include(p => p.BlogNotices)
            .Include(p => p.QuickLinks)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        var stats = await GetProfileStatsAsync(updated!.Id, updated!.UserId, null, ct);
        return ResultModel<UserProfileResponse>.Ok(MapToResponse(updated!, stats));
    }

    public async Task<ResultModel<BlogThemeDto>> GetThemeAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await uow.BlogProfiles.Query()
            .Include(p => p.Theme)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
            return ResultModel<BlogThemeDto>.NotFound("Profile not found.");

        return ResultModel<BlogThemeDto>.Ok(profile.Theme is null ? new BlogThemeDto() : MapTheme(profile.Theme));
    }

    public async Task<ResultModel<BlogThemeDto>> UpdateThemeAsync(Guid userId, UpdateBlogThemeRequest request, CancellationToken ct = default)
    {
        var profile = await uow.BlogProfiles.Query()
            .Include(p => p.Theme)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
            return ResultModel<BlogThemeDto>.NotFound("Profile not found.");

        var configJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            primaryColor = request.PrimaryColor,
            secondaryColor = request.SecondaryColor,
            accentColor = request.AccentColor,
            fontFamily = request.FontFamily,
            layoutStyle = request.LayoutStyle,
            darkModeDefault = request.DarkModeDefault
        });

        if (profile.Theme is null)
        {
            var theme = new Domain.Entities.Content.BlogTheme
            {
                BlogProfileId = profile.Id,
                Config = System.Text.Json.JsonDocument.Parse(configJson)
            };
            await uow.BlogThemes.AddAsync(theme, ct);
        }
        else
        {
            profile.Theme.Config = System.Text.Json.JsonDocument.Parse(configJson);
            uow.BlogThemes.Update(profile.Theme);
        }

        await uow.SaveChangesAsync(ct);

        var updated = await uow.BlogProfiles.Query()
            .Include(p => p.Theme)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        return ResultModel<BlogThemeDto>.Ok(updated?.Theme is null ? new BlogThemeDto() : MapTheme(updated.Theme));
    }

    public async Task<ResultModel<List<UserWordFilterDto>>> GetWordFiltersAsync(Guid userId, CancellationToken ct = default)
    {
        var filters = await uow.UserWordFilters.Query()
            .Where(f => f.UserId == userId)
            .OrderBy(f => f.Word)
            .ToListAsync(ct);

        return ResultModel<List<UserWordFilterDto>>.Ok(filters.Select(f => new UserWordFilterDto
        {
            Id = f.Id,
            Word = f.Word,
            CreatedAt = f.CreatedAt
        }).ToList());
    }

    public async Task<ResultModel<UserWordFilterDto>> AddWordFilterAsync(Guid userId, AddWordFilterRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Word))
            return ResultModel<UserWordFilterDto>.BadRequest("Word is required.");

        var exists = await uow.UserWordFilters.Query()
            .AnyAsync(f => f.UserId == userId && f.Word.ToLower() == request.Word.ToLower(), ct);

        if (exists)
            return ResultModel<UserWordFilterDto>.Conflict("This word is already in your filter list.");

        var filter = new Domain.Entities.Content.UserWordFilter
        {
            UserId = userId,
            Word = request.Word.Trim().ToLower()
        };

        await uow.UserWordFilters.AddAsync(filter, ct);
        await uow.SaveChangesAsync(ct);

        return ResultModel<UserWordFilterDto>.Created(new UserWordFilterDto
        {
            Id = filter.Id,
            Word = filter.Word,
            CreatedAt = filter.CreatedAt
        });
    }

    public async Task<ResultModel> DeleteWordFilterAsync(Guid userId, Guid filterId, CancellationToken ct = default)
    {
        var filter = await uow.UserWordFilters.Query()
            .FirstOrDefaultAsync(f => f.Id == filterId && f.UserId == userId, ct);

        if (filter is null)
            return ResultModel.NotFound("Filter not found.");

        uow.UserWordFilters.Remove(filter);
        await uow.SaveChangesAsync(ct);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel<List<BlogNoticeDto>>> GetNoticesAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await uow.BlogProfiles.Query()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
            return ResultModel<List<BlogNoticeDto>>.NotFound("Profile not found.");

        var notices = await uow.BlogNotices.Query()
            .Where(n => n.BlogProfileId == profile.Id)
            .OrderByDescending(n => n.Priority)
            .ToListAsync(ct);

        return ResultModel<List<BlogNoticeDto>>.Ok(notices.Select(n => new BlogNoticeDto
        {
            Id = n.Id,
            Title = n.Title,
            Content = n.Content,
            Type = n.Type.ToString(),
            IsActive = n.IsActive,
            ExpiresAt = n.ExpiresAt,
            Priority = n.Priority,
            CreatedAt = n.CreatedAt
        }).ToList());
    }

    public async Task<ResultModel<BlogNoticeDto>> AddNoticeAsync(Guid userId, CreateBlogNoticeRequest request, CancellationToken ct = default)
    {
        var profile = await uow.BlogProfiles.Query()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
            return ResultModel<BlogNoticeDto>.NotFound("Profile not found.");

        if (!Enum.TryParse<Domain.Enums.BlogNoticeType>(request.Type, true, out var noticeType))
            noticeType = Domain.Enums.BlogNoticeType.Info;

        var notice = new Domain.Entities.Content.BlogNotice
        {
            BlogProfileId = profile.Id,
            Title = request.Title,
            Content = request.Content,
            Type = noticeType,
            IsActive = request.IsActive,
            ExpiresAt = request.ExpiresAt,
            Priority = request.Priority
        };

        await uow.BlogNotices.AddAsync(notice, ct);
        await uow.SaveChangesAsync(ct);

        return ResultModel<BlogNoticeDto>.Created(new BlogNoticeDto
        {
            Id = notice.Id,
            Title = notice.Title,
            Content = notice.Content,
            Type = notice.Type.ToString(),
            IsActive = notice.IsActive,
            ExpiresAt = notice.ExpiresAt,
            Priority = notice.Priority,
            CreatedAt = notice.CreatedAt
        });
    }

    public async Task<ResultModel<BlogNoticeDto>> UpdateNoticeAsync(Guid userId, Guid noticeId, CreateBlogNoticeRequest request, CancellationToken ct = default)
    {
        var profile = await uow.BlogProfiles.Query()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
            return ResultModel<BlogNoticeDto>.NotFound("Profile not found.");

        var notice = await uow.BlogNotices.Query()
            .FirstOrDefaultAsync(n => n.Id == noticeId && n.BlogProfileId == profile.Id, ct);

        if (notice is null)
            return ResultModel<BlogNoticeDto>.NotFound("Notice not found.");

        if (!Enum.TryParse<Domain.Enums.BlogNoticeType>(request.Type, true, out var noticeType))
            noticeType = Domain.Enums.BlogNoticeType.Info;

        notice.Title = request.Title;
        notice.Content = request.Content;
        notice.Type = noticeType;
        notice.IsActive = request.IsActive;
        notice.ExpiresAt = request.ExpiresAt;
        notice.Priority = request.Priority;
        notice.UpdatedAt = DateTime.UtcNow;

        uow.BlogNotices.Update(notice);
        await uow.SaveChangesAsync(ct);

        return ResultModel<BlogNoticeDto>.Ok(new BlogNoticeDto
        {
            Id = notice.Id,
            Title = notice.Title,
            Content = notice.Content,
            Type = notice.Type.ToString(),
            IsActive = notice.IsActive,
            ExpiresAt = notice.ExpiresAt,
            Priority = notice.Priority,
            CreatedAt = notice.CreatedAt
        });
    }

    public async Task<ResultModel> DeleteNoticeAsync(Guid userId, Guid noticeId, CancellationToken ct = default)
    {
        var profile = await uow.BlogProfiles.Query()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
            return ResultModel.NotFound("Profile not found.");

        var notice = await uow.BlogNotices.Query()
            .FirstOrDefaultAsync(n => n.Id == noticeId && n.BlogProfileId == profile.Id, ct);

        if (notice is null)
            return ResultModel.NotFound("Notice not found.");

        uow.BlogNotices.Remove(notice);
        await uow.SaveChangesAsync(ct);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel<List<QuickLinkDto>>> GetQuickLinksAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await uow.BlogProfiles.Query()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
            return ResultModel<List<QuickLinkDto>>.NotFound("Profile not found.");

        var links = await uow.QuickLinks.Query()
            .Where(l => l.BlogProfileId == profile.Id)
            .OrderBy(l => l.Order)
            .ToListAsync(ct);

        return ResultModel<List<QuickLinkDto>>.Ok(links.Select(l => new QuickLinkDto
        {
            Id = l.Id,
            Title = l.Title,
            Url = l.Url,
            Icon = l.Icon,
            Order = l.Order
        }).ToList());
    }

    public async Task<ResultModel<QuickLinkDto>> AddQuickLinkAsync(Guid userId, CreateQuickLinkRequest request, CancellationToken ct = default)
    {
        var profile = await uow.BlogProfiles.Query()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
            return ResultModel<QuickLinkDto>.NotFound("Profile not found.");

        var link = new Domain.Entities.Content.QuickLink
        {
            BlogProfileId = profile.Id,
            Title = request.Title,
            Url = request.Url,
            Icon = request.Icon,
            Order = request.Order
        };

        await uow.QuickLinks.AddAsync(link, ct);
        await uow.SaveChangesAsync(ct);

        return ResultModel<QuickLinkDto>.Created(new QuickLinkDto
        {
            Id = link.Id,
            Title = link.Title,
            Url = link.Url,
            Icon = link.Icon,
            Order = link.Order
        });
    }

    public async Task<ResultModel> DeleteQuickLinkAsync(Guid userId, Guid linkId, CancellationToken ct = default)
    {
        var profile = await uow.BlogProfiles.Query()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
            return ResultModel.NotFound("Profile not found.");

        var link = await uow.QuickLinks.Query()
            .FirstOrDefaultAsync(l => l.Id == linkId && l.BlogProfileId == profile.Id, ct);

        if (link is null)
            return ResultModel.NotFound("Quick link not found.");

        uow.QuickLinks.Remove(link);
        await uow.SaveChangesAsync(ct);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel> UpdateLanguageAsync(Guid userId, string language, CancellationToken ct = default)
    {
        var isValid = await uow.Languages.Query()
            .AnyAsync(l => l.Code == language && l.IsActive, ct);

        if (!isValid)
            return ResultModel.BadRequest($"Language '{language}' is not supported.");

        var profile = await uow.BlogProfiles.Query()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
            return ResultModel.NotFound("Profile not found.");

        profile.User.PreferredLanguage = language;
        await uow.SaveChangesAsync(ct);

        return ResultModel.NoContent();
    }

    public async Task<ResultModel<UserPreferencesDto>> GetPreferencesAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await uow.BlogProfiles.Query()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
            return ResultModel<UserPreferencesDto>.NotFound("Profile not found.");

        // Las migraciones que anadieron estas columnas usaron defaultValue: "",
        // por lo que los usuarios anteriores tienen cadenas vacias. Sin este
        // fallback el selector de idioma de Configuracion aparece en blanco.
        return ResultModel<UserPreferencesDto>.Ok(new UserPreferencesDto
        {
            PreferredLanguage = string.IsNullOrWhiteSpace(profile.User.PreferredLanguage)
                ? "en"
                : profile.User.PreferredLanguage,
            ReceiveEmailNotifications  = profile.User.ReceiveEmailNotifications,
            ProfileVisibility = string.IsNullOrWhiteSpace(profile.User.ProfileVisibility)
                ? "public"
                : profile.User.ProfileVisibility,
        });
    }

    public async Task<ResultModel> UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request, CancellationToken ct = default)
    {
        if (request.ProfileVisibility != "public" && request.ProfileVisibility != "followers")
            return ResultModel.BadRequest("ProfileVisibility must be 'public' or 'followers'.");

        var profile = await uow.BlogProfiles.Query()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
            return ResultModel.NotFound("Profile not found.");

        profile.User.ReceiveEmailNotifications = request.ReceiveEmailNotifications;
        profile.User.ProfileVisibility         = request.ProfileVisibility;
        await uow.SaveChangesAsync(ct);

        return ResultModel.NoContent();
    }

    private async Task<(int FollowersCount, int FollowingCount, int PostsCount, bool IsFollowing, bool IsBlocked)> GetProfileStatsAsync(
        Guid profileId, Guid profileUserId, Guid? viewerUserId, CancellationToken ct)
    {
        var followersCount = await uow.Follows.Query()
            .CountAsync(f => f.BlogProfileId == profileId, ct);

        var followingCount = await uow.Follows.Query()
            .CountAsync(f => f.FollowerId == profileUserId, ct);

        var postsCount = await uow.Posts.Query()
            .CountAsync(p => p.BlogProfileId == profileId && p.Status == PostStatus.Published, ct);

        var isFollowing = viewerUserId.HasValue
            && await uow.Follows.Query()
                .AnyAsync(f => f.FollowerId == viewerUserId.Value && f.BlogProfileId == profileId, ct);

        var isBlocked = viewerUserId.HasValue
            && await uow.UserBlocks.Query()
                .AnyAsync(b => b.BlockerId == viewerUserId.Value && b.BlockedId == profileUserId, ct);

        return (followersCount, followingCount, postsCount, isFollowing, isBlocked);
    }

    private static UserProfileResponse MapToResponse(
        BlogProfile profile,
        (int FollowersCount, int FollowingCount, int PostsCount, bool IsFollowing, bool IsBlocked) stats) => new()
    {
        UserId = profile.UserId,
        Username = profile.User.UserName ?? string.Empty,
        DisplayName = profile.User.DisplayName,
        Bio = profile.User.Bio,
        AvatarUrl = profile.User.AvatarUrl,
        Slug = profile.Slug,
        About = profile.About,
        LogoUrl = profile.LogoUrl,
        BannerUrl = profile.BannerUrl,
        Tagline = profile.Tagline,
        Theme = profile.Theme is null ? null : MapTheme(profile.Theme),
        SocialLinks = profile.SocialLinks
            .Select(s => new SocialLinkDto { Id = s.Id, Platform = s.Platform, Url = s.Url })
            .ToList(),
        ActiveNotices = profile.BlogNotices
            .Where(n => n.IsActive && (n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow))
            .OrderByDescending(n => n.Priority)
            .Select(n => new BlogNoticeDto
            {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                Type = n.Type.ToString(),
                IsActive = n.IsActive,
                ExpiresAt = n.ExpiresAt,
                Priority = n.Priority,
                CreatedAt = n.CreatedAt
            })
            .ToList(),
        QuickLinks = profile.QuickLinks
            .OrderBy(q => q.Order)
            .Select(q => new QuickLinkDto
            {
                Id = q.Id,
                Title = q.Title,
                Url = q.Url,
                Icon = q.Icon,
                Order = q.Order
            })
            .ToList(),
        FollowersCount = stats.FollowersCount,
        FollowingCount = stats.FollowingCount,
        PostsCount = stats.PostsCount,
        IsFollowing = stats.IsFollowing,
        IsBlocked = stats.IsBlocked,
        CreatedAt = profile.User.CreatedAt
    };

    private static BlogThemeDto MapTheme(BlogTheme theme)
    {
        try
        {
            var config = theme.Config;
            var root = config.RootElement;
            return new BlogThemeDto
            {
                PrimaryColor = root.TryGetProperty("primaryColor", out var pc) ? pc.GetString() : null,
                SecondaryColor = root.TryGetProperty("secondaryColor", out var sc) ? sc.GetString() : null,
                AccentColor = root.TryGetProperty("accentColor", out var ac) ? ac.GetString() : null,
                FontFamily = root.TryGetProperty("fontFamily", out var ff) ? ff.GetString() : null,
                LayoutStyle = root.TryGetProperty("layoutStyle", out var ls) ? ls.GetString() : null,
                DarkModeDefault = root.TryGetProperty("darkModeDefault", out var dm) && dm.GetBoolean()
            };
        }
        catch
        {
            return new BlogThemeDto();
        }
    }
}
