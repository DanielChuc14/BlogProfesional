using BlogPlatform.Application.DTOs.Community;
using BlogPlatform.Application.Interfaces;
using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Entities.Content;
using BlogPlatform.Domain.Enums;
using BlogPlatform.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Infrastructure.Services;

public class FollowService(
    IUnitOfWork uow,
    ILogger<FollowService> logger) : IFollowService
{
    public async Task<ResultModel> FollowAsync(Guid userId, string profileSlug, CancellationToken ct = default)
    {
        var profile = await uow.BlogProfiles.Query()
            .FirstOrDefaultAsync(bp => bp.Slug == profileSlug, ct);

        if (profile is null)
            return ResultModel.NotFound("Blog profile not found.");

        if (profile.UserId == userId)
            return ResultModel.BadRequest("You cannot follow your own profile.");

        var isBlocked = await uow.UserBlocks.Query()
            .AnyAsync(b => (b.BlockerId == userId && b.BlockedId == profile.UserId) ||
                           (b.BlockerId == profile.UserId && b.BlockedId == userId), ct);

        if (isBlocked)
            return ResultModel.Forbidden("Cannot follow this user.");

        var existing = await uow.Follows.Query()
            .FirstOrDefaultAsync(f => f.FollowerId == userId && f.BlogProfileId == profile.Id, ct);

        if (existing is not null)
            return ResultModel.Conflict("Already following.");

        await uow.Follows.AddAsync(new Follow { FollowerId = userId, BlogProfileId = profile.Id }, ct);

        await uow.Notifications.AddAsync(new Notification
        {
            RecipientId = profile.UserId,
            ActorId = userId,
            Type = NotificationType.NewFollower
        }, ct);

        await uow.SaveChangesAsync(ct);

        logger.LogInformation("User {UserId} followed profile {ProfileSlug}", userId, profileSlug);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel> UnfollowAsync(Guid userId, string profileSlug, CancellationToken ct = default)
    {
        var profile = await uow.BlogProfiles.Query()
            .FirstOrDefaultAsync(bp => bp.Slug == profileSlug, ct);

        if (profile is null)
            return ResultModel.NotFound("Blog profile not found.");

        var follow = await uow.Follows.Query()
            .FirstOrDefaultAsync(f => f.FollowerId == userId && f.BlogProfileId == profile.Id, ct);

        if (follow is null)
            return ResultModel.NotFound("Not following this profile.");

        uow.Follows.Remove(follow);
        await uow.SaveChangesAsync(ct);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel<IReadOnlyList<FollowerDto>>> GetFollowersAsync(string profileSlug, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var profile = await uow.BlogProfiles.Query()
            .FirstOrDefaultAsync(bp => bp.Slug == profileSlug, ct);

        if (profile is null)
            return ResultModel<IReadOnlyList<FollowerDto>>.NotFound("Blog profile not found.");

        var followers = await uow.Follows.Query()
            .Include(f => f.Follower).ThenInclude(u => u.Profile)
            .Where(f => f.BlogProfileId == profile.Id)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var result = followers.Select(f => new FollowerDto
        {
            UserId = f.FollowerId,
            Username = f.Follower.UserName ?? string.Empty,
            DisplayName = f.Follower.DisplayName,
            AvatarUrl = f.Follower.AvatarUrl,
            ProfileSlug = f.Follower.Profile?.Slug ?? string.Empty
        }).ToList();

        return ResultModel<IReadOnlyList<FollowerDto>>.Ok(result);
    }

    public async Task<ResultModel<IReadOnlyList<FollowerDto>>> GetFollowingAsync(string profileSlug, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var user = await uow.BlogProfiles.Query()
            .Include(bp => bp.User)
            .FirstOrDefaultAsync(bp => bp.Slug == profileSlug, ct);

        if (user is null)
            return ResultModel<IReadOnlyList<FollowerDto>>.NotFound("User not found.");

        var following = await uow.Follows.Query()
            .Include(f => f.BlogProfile).ThenInclude(bp => bp.User)
            .Where(f => f.FollowerId == user.UserId)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var result = following.Select(f => new FollowerDto
        {
            UserId = f.BlogProfile.UserId,
            Username = f.BlogProfile.User.UserName ?? string.Empty,
            DisplayName = f.BlogProfile.User.DisplayName,
            AvatarUrl = f.BlogProfile.User.AvatarUrl,
            ProfileSlug = f.BlogProfile.Slug
        }).ToList();

        return ResultModel<IReadOnlyList<FollowerDto>>.Ok(result);
    }
}
