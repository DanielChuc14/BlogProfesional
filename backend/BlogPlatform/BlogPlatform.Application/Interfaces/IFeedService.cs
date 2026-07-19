using BlogPlatform.Application.DTOs.Common;
using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Domain.Common;

namespace BlogPlatform.Application.Interfaces;

public interface IFeedService
{
    Task<ResultModel<CursorPagedResult<PostSummaryDto>>> GetPersonalizedFeedAsync(Guid userId, string? cursor, int pageSize, bool includeAdultContent = false, CancellationToken ct = default);
}
