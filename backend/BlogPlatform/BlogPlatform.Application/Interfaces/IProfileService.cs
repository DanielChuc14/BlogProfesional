using BlogPlatform.Application.DTOs.Media;
using BlogPlatform.Application.DTOs.Profile;
using BlogPlatform.Domain.Common;

namespace BlogPlatform.Application.Interfaces;

public interface IProfileService
{
    Task<ResultModel<UserProfileResponse>> GetByUsernameAsync(string username, Guid? viewerUserId = null, CancellationToken ct = default);
    Task<ResultModel<UserProfileResponse>> UpdateAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);
    Task<ResultModel<UserProfileResponse>> UpdateAvatarAsync(Guid userId, FileUploadDto file, CancellationToken ct = default);
    Task<ResultModel<UserProfileResponse>> UpdateBannerAsync(Guid userId, FileUploadDto file, CancellationToken ct = default);

    // Theme
    Task<ResultModel<BlogThemeDto>> GetThemeAsync(Guid userId, CancellationToken ct = default);
    Task<ResultModel<BlogThemeDto>> UpdateThemeAsync(Guid userId, UpdateBlogThemeRequest request, CancellationToken ct = default);

    // Word filters
    Task<ResultModel<List<UserWordFilterDto>>> GetWordFiltersAsync(Guid userId, CancellationToken ct = default);
    Task<ResultModel<UserWordFilterDto>> AddWordFilterAsync(Guid userId, AddWordFilterRequest request, CancellationToken ct = default);
    Task<ResultModel> DeleteWordFilterAsync(Guid userId, Guid filterId, CancellationToken ct = default);

    // Notices
    Task<ResultModel<List<BlogNoticeDto>>> GetNoticesAsync(Guid userId, CancellationToken ct = default);
    Task<ResultModel<BlogNoticeDto>> AddNoticeAsync(Guid userId, CreateBlogNoticeRequest request, CancellationToken ct = default);
    Task<ResultModel<BlogNoticeDto>> UpdateNoticeAsync(Guid userId, Guid noticeId, CreateBlogNoticeRequest request, CancellationToken ct = default);
    Task<ResultModel> DeleteNoticeAsync(Guid userId, Guid noticeId, CancellationToken ct = default);

    // Quick links
    Task<ResultModel<List<QuickLinkDto>>> GetQuickLinksAsync(Guid userId, CancellationToken ct = default);
    Task<ResultModel<QuickLinkDto>> AddQuickLinkAsync(Guid userId, CreateQuickLinkRequest request, CancellationToken ct = default);
    Task<ResultModel> DeleteQuickLinkAsync(Guid userId, Guid linkId, CancellationToken ct = default);

    // Language preference
    Task<ResultModel> UpdateLanguageAsync(Guid userId, string language, CancellationToken ct = default);

    // Preferences
    Task<ResultModel<UserPreferencesDto>> GetPreferencesAsync(Guid userId, CancellationToken ct = default);
    Task<ResultModel> UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request, CancellationToken ct = default);
}
