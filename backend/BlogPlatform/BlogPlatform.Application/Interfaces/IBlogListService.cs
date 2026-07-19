using BlogPlatform.Application.DTOs.Lists;
using BlogPlatform.Domain.Common;

namespace BlogPlatform.Application.Interfaces;

public interface IBlogListService
{
    Task<ResultModel<List<BlogListSummaryDto>>> GetByProfileAsync(string username, bool includePrivate, CancellationToken ct = default);
    Task<ResultModel<BlogListDto>> GetBySlugAsync(string username, string slug, CancellationToken ct = default);
    Task<ResultModel<BlogListDto>> CreateAsync(Guid userId, CreateBlogListRequest request, CancellationToken ct = default);
    Task<ResultModel<BlogListDto>> UpdateAsync(Guid userId, Guid listId, UpdateBlogListRequest request, CancellationToken ct = default);
    Task<ResultModel> DeleteAsync(Guid userId, Guid listId, CancellationToken ct = default);
    Task<ResultModel> AddPostAsync(Guid userId, Guid listId, AddPostToListRequest request, CancellationToken ct = default);
    Task<ResultModel> RemovePostAsync(Guid userId, Guid listId, Guid postId, CancellationToken ct = default);
}
