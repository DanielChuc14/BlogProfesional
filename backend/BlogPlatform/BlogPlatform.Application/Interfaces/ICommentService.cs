using BlogPlatform.Application.DTOs.Community;
using BlogPlatform.Domain.Common;

namespace BlogPlatform.Application.Interfaces;

public interface ICommentService
{
    Task<ResultModel<IReadOnlyList<CommentDto>>> GetByPostAsync(Guid postId, CancellationToken ct = default);
    Task<ResultModel<CommentDto>> CreateAsync(Guid userId, Guid postId, CreateCommentRequest request, CancellationToken ct = default);
    Task<ResultModel<CommentDto>> UpdateAsync(Guid userId, Guid commentId, UpdateCommentRequest request, CancellationToken ct = default);
    Task<ResultModel> DeleteAsync(Guid userId, Guid commentId, CancellationToken ct = default);
    Task<ResultModel> LikeAsync(Guid userId, Guid commentId, CancellationToken ct = default);
    Task<ResultModel> UnlikeAsync(Guid userId, Guid commentId, CancellationToken ct = default);
}
