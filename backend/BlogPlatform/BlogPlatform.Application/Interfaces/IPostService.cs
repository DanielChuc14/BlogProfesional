using BlogPlatform.Application.DTOs.Common;
using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Domain.Common;

namespace BlogPlatform.Application.Interfaces;

public interface IPostService
{
    Task<ResultModel<CursorPagedResult<PostSummaryDto>>> GetFeedAsync(PostFeedQuery query, CancellationToken ct = default);
    Task<ResultModel<PostDetailDto>> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<ResultModel<PostDetailDto>> GetByIdAsync(Guid userId, Guid postId, CancellationToken ct = default);
    Task<ResultModel<PostDetailDto>> CreateAsync(Guid userId, CreatePostRequest request, CancellationToken ct = default);
    Task<ResultModel<PostDetailDto>> UpdateAsync(Guid userId, Guid postId, UpdatePostRequest request, CancellationToken ct = default);
    Task<ResultModel> DeleteAsync(Guid userId, Guid postId, CancellationToken ct = default);
    Task<ResultModel<PostDetailDto>> PublishAsync(Guid userId, Guid postId, CancellationToken ct = default);
    Task<ResultModel<PostDetailDto>> ScheduleAsync(Guid userId, Guid postId, SchedulePostRequest request, CancellationToken ct = default);
    Task<ResultModel<PostDetailDto>> ArchiveAsync(Guid userId, Guid postId, CancellationToken ct = default);
    Task<ResultModel<CursorPagedResult<PostSummaryDto>>> SearchAsync(SearchQuery query, CancellationToken ct = default);
    Task<ResultModel> LikePostAsync(Guid userId, Guid postId, CancellationToken ct = default);
    Task<ResultModel> UnlikePostAsync(Guid userId, Guid postId, CancellationToken ct = default);
    Task<ResultModel<PostDetailDto>> ChangeSlugAsync(Guid userId, Guid postId, string newSlug, CancellationToken ct = default);
}
