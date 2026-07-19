using BlogPlatform.Application.DTOs.Security;
using BlogPlatform.Application.Interfaces;
using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Entities.Security;
using BlogPlatform.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Infrastructure.Services;

public class BlockService(
    IUnitOfWork uow,
    ILogger<BlockService> logger) : IBlockService
{
    public async Task<ResultModel> BlockUserAsync(Guid blockerId, Guid targetUserId, CancellationToken ct = default)
    {
        if (blockerId == targetUserId)
            return ResultModel.BadRequest("You cannot block yourself.");

        var existing = await uow.UserBlocks.Query()
            .FirstOrDefaultAsync(b => b.BlockerId == blockerId && b.BlockedId == targetUserId, ct);

        if (existing is not null)
            return ResultModel.Conflict("User is already blocked.");

        await uow.UserBlocks.AddAsync(new UserBlock { BlockerId = blockerId, BlockedId = targetUserId }, ct);

        // Remove follows in both directions
        var blockerProfile = await uow.BlogProfiles.Query()
            .FirstOrDefaultAsync(bp => bp.UserId == blockerId, ct);

        var targetProfile = await uow.BlogProfiles.Query()
            .FirstOrDefaultAsync(bp => bp.UserId == targetUserId, ct);

        if (targetProfile is not null)
        {
            var blockerFollowsTarget = await uow.Follows.Query()
                .FirstOrDefaultAsync(f => f.FollowerId == blockerId && f.BlogProfileId == targetProfile.Id, ct);
            if (blockerFollowsTarget is not null)
                uow.Follows.Remove(blockerFollowsTarget);
        }

        if (blockerProfile is not null)
        {
            var targetFollowsBlocker = await uow.Follows.Query()
                .FirstOrDefaultAsync(f => f.FollowerId == targetUserId && f.BlogProfileId == blockerProfile.Id, ct);
            if (targetFollowsBlocker is not null)
                uow.Follows.Remove(targetFollowsBlocker);
        }

        await uow.SaveChangesAsync(ct);

        logger.LogInformation("User {BlockerId} blocked user {TargetUserId}", blockerId, targetUserId);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel> UnblockUserAsync(Guid blockerId, Guid targetUserId, CancellationToken ct = default)
    {
        var block = await uow.UserBlocks.Query()
            .FirstOrDefaultAsync(b => b.BlockerId == blockerId && b.BlockedId == targetUserId, ct);

        if (block is null)
            return ResultModel.NotFound("Block not found.");

        uow.UserBlocks.Remove(block);
        await uow.SaveChangesAsync(ct);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel<IReadOnlyList<BlockedUserDto>>> GetBlockedUsersAsync(
        Guid userId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var blocks = await uow.UserBlocks.Query()
            .Include(b => b.Blocked)
            .Where(b => b.BlockerId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var result = blocks.Select(b => new BlockedUserDto
        {
            UserId = b.BlockedId,
            Username = b.Blocked.UserName ?? string.Empty,
            DisplayName = b.Blocked.DisplayName,
            AvatarUrl = b.Blocked.AvatarUrl,
            BlockedAt = b.CreatedAt
        }).ToList();

        return ResultModel<IReadOnlyList<BlockedUserDto>>.Ok(result);
    }

    public async Task<bool> IsBlockedAsync(Guid userId, Guid targetUserId, CancellationToken ct = default)
    {
        return await uow.UserBlocks.Query()
            .AnyAsync(b => (b.BlockerId == userId && b.BlockedId == targetUserId) ||
                           (b.BlockerId == targetUserId && b.BlockedId == userId), ct);
    }
}
