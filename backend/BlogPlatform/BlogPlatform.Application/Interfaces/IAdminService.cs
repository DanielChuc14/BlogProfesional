using BlogPlatform.Application.DTOs.Admin;
using BlogPlatform.Application.DTOs.Common;
using BlogPlatform.Application.DTOs.Security;
using BlogPlatform.Application.DTOs.Tags;
using BlogPlatform.Domain.Common;

namespace BlogPlatform.Application.Interfaces;

public interface IAdminService
{
    // Users
    Task<ResultModel<PagedResult<UserSummaryDto>>> GetUsersAsync(int page, int pageSize, string? search, CancellationToken ct = default);
    Task<ResultModel<UserSummaryDto>> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
    Task<ResultModel> DeleteUserAsync(Guid userId, CancellationToken ct = default);
    Task<ResultModel> BanUserAsync(Guid targetUserId, CancellationToken ct = default);
    Task<ResultModel> UnbanUserAsync(Guid targetUserId, CancellationToken ct = default);

    // Roles
    Task<ResultModel<List<RoleDto>>> GetRolesAsync(CancellationToken ct = default);
    Task<ResultModel<RoleDto>> CreateRoleAsync(string name, CancellationToken ct = default);
    Task<ResultModel> DeleteRoleAsync(string name, CancellationToken ct = default);
    Task<ResultModel> AssignRoleAsync(Guid userId, string role, CancellationToken ct = default);
    Task<ResultModel> RemoveRoleAsync(Guid userId, string role, CancellationToken ct = default);

    // Posts
    Task<ResultModel<PagedResult<AdminPostSummaryDto>>> GetAllPostsAsync(int page, int pageSize, string? status, CancellationToken ct = default);
    Task<ResultModel> ForceDeletePostAsync(Guid postId, CancellationToken ct = default);
    Task<ResultModel> UnpublishPostAsync(Guid adminId, Guid postId, string reason, CancellationToken ct = default);

    // Comments (moderation)
    Task<ResultModel<PagedResult<CommentSummaryDto>>> GetCommentsAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ResultModel> DeleteCommentAsync(Guid commentId, CancellationToken ct = default);

    // Tags
    Task<ResultModel<List<TagDto>>> GetTagsAsync(CancellationToken ct = default);
    Task<ResultModel<TagDto>> CreateTagAsync(string name, CancellationToken ct = default);
    Task<ResultModel<TagDto>> UpdateTagAsync(Guid id, string name, CancellationToken ct = default);
    Task<ResultModel> DeleteTagAsync(Guid tagId, CancellationToken ct = default);

    // Suspensions
    Task<ResultModel> SuspendUserAsync(Guid adminId, Guid userId, SuspendUserRequest request, CancellationToken ct = default);
    Task<ResultModel> LiftSuspensionAsync(Guid adminId, Guid userId, CancellationToken ct = default);
    Task<ResultModel<List<UserSuspensionDto>>> GetSuspensionHistoryAsync(Guid userId, CancellationToken ct = default);

    // Restricted Words
    Task<ResultModel<List<RestrictedWordDto>>> GetRestrictedWordsAsync(CancellationToken ct = default);
    Task<ResultModel<RestrictedWordDto>> AddRestrictedWordAsync(Guid adminId, AddRestrictedWordRequest request, CancellationToken ct = default);
    Task<ResultModel> DeleteRestrictedWordAsync(Guid wordId, CancellationToken ct = default);

    // Audit log timeline
    Task<ResultModel<PagedResult<AuditLogDto>>> GetAuditLogsAsync(int page, int pageSize, CancellationToken ct = default);
}
