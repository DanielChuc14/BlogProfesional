using BlogPlatform.Application.DTOs.Security;
using BlogPlatform.Domain.Common;

namespace BlogPlatform.Application.Interfaces;

public interface IBlockService
{
    Task<ResultModel> BlockUserAsync(Guid blockerId, Guid targetUserId, CancellationToken ct = default);
    Task<ResultModel> UnblockUserAsync(Guid blockerId, Guid targetUserId, CancellationToken ct = default);
    Task<ResultModel<IReadOnlyList<BlockedUserDto>>> GetBlockedUsersAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<bool> IsBlockedAsync(Guid userId, Guid targetUserId, CancellationToken ct = default);
}
