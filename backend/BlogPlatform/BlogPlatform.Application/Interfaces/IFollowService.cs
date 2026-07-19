using BlogPlatform.Application.DTOs.Community;
using BlogPlatform.Domain.Common;

namespace BlogPlatform.Application.Interfaces;

public interface IFollowService
{
    Task<ResultModel> FollowAsync(Guid userId, string profileSlug, CancellationToken ct = default);
    Task<ResultModel> UnfollowAsync(Guid userId, string profileSlug, CancellationToken ct = default);
    Task<ResultModel<IReadOnlyList<FollowerDto>>> GetFollowersAsync(string profileSlug, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<ResultModel<IReadOnlyList<FollowerDto>>> GetFollowingAsync(string profileSlug, int page = 1, int pageSize = 20, CancellationToken ct = default);
}
