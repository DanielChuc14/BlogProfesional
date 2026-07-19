using BlogPlatform.Application.DTOs.Common;
using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Application.DTOs.Tags;
using BlogPlatform.Domain.Common;

namespace BlogPlatform.Application.Interfaces;

public interface ITagService
{
    Task<ResultModel<List<TagDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ResultModel<TagDto>> CreateAsync(string name, CancellationToken ct = default);
    Task<ResultModel<TagDto>> UpdateAsync(Guid id, string name, CancellationToken ct = default);
    Task<ResultModel<List<TagDto>>> AutocompleteAsync(string query, CancellationToken ct = default);
    Task<ResultModel<CursorPagedResult<PostSummaryDto>>> GetPostsByTagAsync(string tagSlug, string? cursor, int pageSize, CancellationToken ct = default);
}
