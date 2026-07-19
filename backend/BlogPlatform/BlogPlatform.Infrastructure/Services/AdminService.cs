using BlogPlatform.Application.DTOs.Admin;
using BlogPlatform.Application.DTOs.Common;
using BlogPlatform.Application.DTOs.Security;
using BlogPlatform.Application.DTOs.Tags;
using BlogPlatform.Domain.Entities.Content;
using BlogPlatform.Domain.Entities.Security;
using BlogPlatform.Application.Interfaces;
using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Enums;
using BlogPlatform.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Infrastructure.Services;

public class AdminService(
    IUnitOfWork uow,
    UserManager<Domain.Entities.Auth.ApplicationUser> userManager,
    RoleManager<Domain.Entities.Auth.ApplicationRole> roleManager,
    ITagService tagService,
    IAuditService auditService,
    ILogger<AdminService> logger) : IAdminService
{
    public async Task<ResultModel<PagedResult<UserSummaryDto>>> GetUsersAsync(
        int page, int pageSize, string? search, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = userManager.Users.Include(u => u.Profile).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(u =>
                (u.UserName != null && u.UserName.ToLower().Contains(s)) ||
                u.DisplayName.ToLower().Contains(s) ||
                (u.Email != null && u.Email.ToLower().Contains(s)));
        }

        var total = await q.CountAsync(ct);
        var users = await q
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = new List<UserSummaryDto>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            items.Add(new UserSummaryDto
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                DisplayName = user.DisplayName,
                Email = user.Email ?? string.Empty,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles.ToList(),
                ProfileSlug = user.Profile?.Slug,
                SuspendedUntil = user.SuspendedUntil,
                CreatedAt = user.CreatedAt
            });
        }

        return ResultModel<PagedResult<UserSummaryDto>>.Ok(new PagedResult<UserSummaryDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        });
    }

    public async Task<ResultModel<UserSummaryDto>> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null) return ResultModel<UserSummaryDto>.NotFound("User not found.");

        var roles = await userManager.GetRolesAsync(user);
        return ResultModel<UserSummaryDto>.Ok(new UserSummaryDto
        {
            Id = user.Id,
            Username = user.UserName ?? string.Empty,
            DisplayName = user.DisplayName,
            Email = user.Email ?? string.Empty,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            Roles = roles.ToList(),
            ProfileSlug = user.Profile?.Slug,
            SuspendedUntil = user.SuspendedUntil,
            CreatedAt = user.CreatedAt
        });
    }

    public async Task<ResultModel> DeleteUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return ResultModel.NotFound("User not found.");

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ResultModel.Conflict(errors);
        }

        logger.LogInformation("Admin deleted user {UserId}", userId);
        await auditService.LogAsync("DeleteUser", "ApplicationUser", userId.ToString(), ct: ct);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel> BanUserAsync(Guid targetUserId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(targetUserId.ToString());
        if (user is null) return ResultModel.NotFound("User not found.");

        if (!user.IsActive) return ResultModel.Conflict("User is already banned.");

        user.IsActive = false;
        await userManager.UpdateAsync(user);
        await userManager.UpdateSecurityStampAsync(user);

        logger.LogInformation("User {UserId} was banned", targetUserId);
        await auditService.LogAsync("BanUser", "ApplicationUser", targetUserId.ToString(), ct: ct);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel> UnbanUserAsync(Guid targetUserId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(targetUserId.ToString());
        if (user is null) return ResultModel.NotFound("User not found.");

        if (user.IsActive) return ResultModel.Conflict("User is not banned.");

        user.IsActive = true;
        await userManager.UpdateAsync(user);

        logger.LogInformation("User {UserId} was unbanned", targetUserId);
        await auditService.LogAsync("UnbanUser", "ApplicationUser", targetUserId.ToString(), ct: ct);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel<List<RoleDto>>> GetRolesAsync(CancellationToken ct = default)
    {
        var roles = await roleManager.Roles.ToListAsync(ct);
        var result = new List<RoleDto>();

        foreach (var role in roles.OrderBy(r => r.Name))
        {
            var users = await userManager.GetUsersInRoleAsync(role.Name!);
            result.Add(new RoleDto { Name = role.Name!, UserCount = users.Count });
        }

        return ResultModel<List<RoleDto>>.Ok(result);
    }

    public async Task<ResultModel<RoleDto>> CreateRoleAsync(string name, CancellationToken ct = default)
    {
        name = name.Trim();
        if (await roleManager.RoleExistsAsync(name))
            return ResultModel<RoleDto>.Conflict($"Role '{name}' already exists.");

        var result = await roleManager.CreateAsync(new Domain.Entities.Auth.ApplicationRole(name));
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ResultModel<RoleDto>.Conflict(errors);
        }

        logger.LogInformation("Role created: {Role}", name);
        return ResultModel<RoleDto>.Created(new RoleDto { Name = name, UserCount = 0 });
    }

    public async Task<ResultModel> DeleteRoleAsync(string name, CancellationToken ct = default)
    {
        var role = await roleManager.FindByNameAsync(name);
        if (role is null) return ResultModel.NotFound($"Role '{name}' not found.");

        var result = await roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ResultModel.Conflict(errors);
        }

        logger.LogInformation("Role deleted: {Role}", name);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel> AssignRoleAsync(Guid userId, string role, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return ResultModel.NotFound("User not found.");

        if (!await roleManager.RoleExistsAsync(role))
            return ResultModel.NotFound($"Role '{role}' not found.");

        if (await userManager.IsInRoleAsync(user, role))
            return ResultModel.Conflict($"User already has role '{role}'.");

        await userManager.AddToRoleAsync(user, role);
        logger.LogInformation("Role {Role} assigned to user {UserId}", role, userId);
        await auditService.LogAsync("AssignRole", "ApplicationUser", userId.ToString(), reason: role, ct: ct);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel> RemoveRoleAsync(Guid userId, string role, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return ResultModel.NotFound("User not found.");

        if (!await userManager.IsInRoleAsync(user, role))
            return ResultModel.Conflict($"User does not have role '{role}'.");

        await userManager.RemoveFromRoleAsync(user, role);
        logger.LogInformation("Role {Role} removed from user {UserId}", role, userId);
        await auditService.LogAsync("RemoveRole", "ApplicationUser", userId.ToString(), reason: role, ct: ct);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel<PagedResult<AdminPostSummaryDto>>> GetAllPostsAsync(
        int page, int pageSize, string? status, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        IQueryable<Domain.Entities.Content.Post> q = uow.Posts.Query()
            .Include(p => p.BlogProfile).ThenInclude(bp => bp.User);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PostStatus>(status, true, out var postStatus))
            q = q.Where(p => p.Status == postStatus);

        var total = await q.CountAsync(ct);
        var posts = await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = posts.Select(p => new AdminPostSummaryDto
        {
            Id = p.Id,
            Title = p.Title,
            Slug = p.Slug,
            Status = p.Status.ToString(),
            AuthorUsername = p.BlogProfile.User.UserName ?? string.Empty,
            ViewCount = p.ViewCount,
            LikesCount = p.LikesCount,
            CommentsCount = p.CommentsCount,
            PublishedAt = p.PublishedAt,
            CreatedAt = p.CreatedAt
        }).ToList();

        return ResultModel<PagedResult<AdminPostSummaryDto>>.Ok(new PagedResult<AdminPostSummaryDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        });
    }

    public async Task<ResultModel> ForceDeletePostAsync(Guid postId, CancellationToken ct = default)
    {
        var post = await uow.Posts.GetByIdAsync(postId, ct);
        if (post is null) return ResultModel.NotFound("Post not found.");

        uow.Posts.Remove(post);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Admin force-deleted post {PostId}", postId);
        await auditService.LogAsync("ForceDeletePost", "Post", postId.ToString(), ct: ct);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel> UnpublishPostAsync(Guid adminId, Guid postId, string reason, CancellationToken ct = default)
    {
        var post = await uow.Posts.Query()
            .Include(p => p.BlogProfile)
            .FirstOrDefaultAsync(p => p.Id == postId, ct);

        if (post is null) return ResultModel.NotFound("Post not found.");

        post.Status = PostStatus.Archived;
        uow.Posts.Update(post);

        await uow.Notifications.AddAsync(new Notification
        {
            RecipientId = post.BlogProfile.UserId,
            ActorId     = adminId,
            Type        = NotificationType.PostUnpublished,
            PostId      = post.Id
        }, ct);

        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Admin unpublished post {PostId}. Reason: {Reason}", postId, reason);
        await auditService.LogAsync("UnpublishPost", "Post", postId.ToString(), reason: reason, ct: ct);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel<PagedResult<CommentSummaryDto>>> GetCommentsAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = uow.Comments.Query()
            .Include(c => c.Author)
            .Include(c => c.Post);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CommentSummaryDto
            {
                Id = c.Id,
                Body = c.Body.Length > 200 ? c.Body.Substring(0, 200) + "…" : c.Body,
                AuthorUsername = c.Author.UserName ?? string.Empty,
                PostTitle = c.Post.Title,
                PostSlug = c.Post.Slug,
                IsDeleted = c.IsDeleted,
                LikesCount = c.LikesCount,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(ct);

        return ResultModel<PagedResult<CommentSummaryDto>>.Ok(new PagedResult<CommentSummaryDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        });
    }

    public async Task<ResultModel> DeleteCommentAsync(Guid commentId, CancellationToken ct = default)
    {
        var comment = await uow.Comments.GetByIdAsync(commentId, ct);
        if (comment is null) return ResultModel.NotFound("Comment not found.");

        uow.Comments.Remove(comment);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Admin deleted comment {CommentId}", commentId);
        await auditService.LogAsync("DeleteComment", "Comment", commentId.ToString(), ct: ct);
        return ResultModel.NoContent();
    }

    public Task<ResultModel<List<TagDto>>> GetTagsAsync(CancellationToken ct = default)
        => tagService.GetAllAsync(ct);

    public Task<ResultModel<TagDto>> CreateTagAsync(string name, CancellationToken ct = default)
        => tagService.CreateAsync(name, ct);

    public Task<ResultModel<TagDto>> UpdateTagAsync(Guid id, string name, CancellationToken ct = default)
        => tagService.UpdateAsync(id, name, ct);

    public async Task<ResultModel> DeleteTagAsync(Guid tagId, CancellationToken ct = default)
    {
        var tag = await uow.Tags.GetByIdAsync(tagId, ct);
        if (tag is null) return ResultModel.NotFound("Tag not found.");

        uow.Tags.Remove(tag);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Admin deleted tag {TagId} ({TagName})", tagId, tag.Name);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel<List<RestrictedWordDto>>> GetRestrictedWordsAsync(CancellationToken ct = default)
    {
        var words = await uow.RestrictedWords.Query()
            .OrderBy(w => w.Phrase)
            .ToListAsync(ct);

        return ResultModel<List<RestrictedWordDto>>.Ok(words.Select(w => new RestrictedWordDto
        {
            Id        = w.Id,
            Phrase    = w.Phrase,
            IsRegex   = w.IsRegex,
            Severity  = w.Severity.ToString(),
            CreatedAt = w.CreatedAt
        }).ToList());
    }

    public async Task<ResultModel<RestrictedWordDto>> AddRestrictedWordAsync(
        Guid adminId, AddRestrictedWordRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Phrase))
            return ResultModel<RestrictedWordDto>.BadRequest("Phrase is required.");

        var exists = await uow.RestrictedWords.Query()
            .AnyAsync(w => w.Phrase.ToLower() == request.Phrase.ToLower(), ct);

        if (exists)
            return ResultModel<RestrictedWordDto>.Conflict("This word or phrase already exists.");

        if (!Enum.TryParse<RestrictedWordSeverity>(request.Severity, true, out var severity))
            severity = RestrictedWordSeverity.Block;

        var word = new Domain.Entities.Admin.RestrictedWord
        {
            Phrase        = request.Phrase.Trim(),
            IsRegex       = request.IsRegex,
            Severity      = severity,
            AddedByUserId = adminId
        };

        await uow.RestrictedWords.AddAsync(word, ct);
        await uow.SaveChangesAsync(ct);

        return ResultModel<RestrictedWordDto>.Created(new RestrictedWordDto
        {
            Id        = word.Id,
            Phrase    = word.Phrase,
            IsRegex   = word.IsRegex,
            Severity  = word.Severity.ToString(),
            CreatedAt = word.CreatedAt
        });
    }

    public async Task<ResultModel> DeleteRestrictedWordAsync(Guid wordId, CancellationToken ct = default)
    {
        var word = await uow.RestrictedWords.GetByIdAsync(wordId, ct);
        if (word is null)
            return ResultModel.NotFound("Restricted word not found.");

        uow.RestrictedWords.Remove(word);
        await uow.SaveChangesAsync(ct);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel> SuspendUserAsync(
        Guid adminId, Guid userId, SuspendUserRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return ResultModel.BadRequest("Reason is required.");

        if (request.DurationDays < 1)
            return ResultModel.BadRequest("Duration must be at least 1 day.");

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return ResultModel.NotFound("User not found.");

        var activeSuspension = await uow.UserSuspensions.Query()
            .AnyAsync(s => s.UserId == userId && s.IsActive, ct);

        if (activeSuspension)
            return ResultModel.Conflict("User is already suspended.");

        var expiresAt = DateTime.UtcNow.AddDays(request.DurationDays);

        var suspension = new UserSuspension
        {
            UserId = userId,
            SuspendedByAdminId = adminId,
            Reason = request.Reason.Trim(),
            ExpiresAt = expiresAt
        };

        user.SuspendedUntil = expiresAt;

        await uow.UserSuspensions.AddAsync(suspension, ct);

        await uow.Notifications.AddAsync(new Notification
        {
            RecipientId = userId,
            ActorId     = adminId,
            Type        = NotificationType.AccountSuspended
        }, ct);

        await userManager.UpdateAsync(user);
        await userManager.UpdateSecurityStampAsync(user);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Admin {AdminId} suspended user {UserId} until {ExpiresAt}", adminId, userId, expiresAt);
        await auditService.LogAsync("SuspendUser", "ApplicationUser", userId.ToString(), reason: request.Reason, ct: ct);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel> LiftSuspensionAsync(Guid adminId, Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return ResultModel.NotFound("User not found.");

        var suspension = await uow.UserSuspensions.Query()
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive, ct);

        if (suspension is null)
            return ResultModel.Conflict("User has no active suspension.");

        suspension.IsActive = false;
        suspension.LiftedByAdminId = adminId;
        suspension.LiftedAt = DateTime.UtcNow;
        user.SuspendedUntil = null;

        uow.UserSuspensions.Update(suspension);
        await userManager.UpdateAsync(user);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Admin {AdminId} lifted suspension for user {UserId}", adminId, userId);
        await auditService.LogAsync("LiftSuspension", "ApplicationUser", userId.ToString(), ct: ct);
        return ResultModel.NoContent();
    }

    public async Task<ResultModel<List<UserSuspensionDto>>> GetSuspensionHistoryAsync(
        Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return ResultModel<List<UserSuspensionDto>>.NotFound("User not found.");

        var suspensions = await uow.UserSuspensions.Query()
            .Include(s => s.SuspendedByAdmin)
            .Include(s => s.LiftedByAdmin)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

        var result = suspensions.Select(s => new UserSuspensionDto
        {
            Id = s.Id,
            Reason = s.Reason,
            ExpiresAt = s.ExpiresAt,
            IsActive = s.IsActive,
            SuspendedByUsername = s.SuspendedByAdmin.UserName ?? string.Empty,
            LiftedByUsername = s.LiftedByAdmin?.UserName,
            LiftedAt = s.LiftedAt,
            CreatedAt = s.CreatedAt
        }).ToList();

        return ResultModel<List<UserSuspensionDto>>.Ok(result);
    }

    public async Task<ResultModel<PagedResult<AuditLogDto>>> GetAuditLogsAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        page     = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var total = await uow.AuditLogs.Query().CountAsync(ct);

        var logs = await uow.AuditLogs.Query()
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var actorIds = logs.Select(l => l.ActorId).Distinct().ToList();
        var actors   = await userManager.Users
            .Where(u => actorIds.Contains(u.Id))
            .Select(u => new { u.Id, u.UserName })
            .ToListAsync(ct);

        var actorMap = actors.ToDictionary(a => a.Id, a => a.UserName ?? string.Empty);

        var items = logs.Select(l => new AuditLogDto
        {
            Id          = l.Id,
            ActorId     = l.ActorId,
            ActorUsername = actorMap.GetValueOrDefault(l.ActorId, l.ActorId.ToString()),
            Action      = l.Action,
            EntityType  = l.EntityType,
            EntityId    = l.EntityId,
            Reason      = l.Reason,
            CreatedAt   = l.CreatedAt
        }).ToList();

        return ResultModel<PagedResult<AuditLogDto>>.Ok(new PagedResult<AuditLogDto>
        {
            Items      = items,
            Total      = total,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        });
    }
}
