using BlogPlatform.Application.DTOs.Common;
using BlogPlatform.Application.DTOs.Security;
using BlogPlatform.Application.Interfaces;
using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Entities.Security;
using BlogPlatform.Domain.Enums;
using BlogPlatform.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Infrastructure.Services;

public class ReportService(
    IUnitOfWork uow,
    ILogger<ReportService> logger) : IReportService
{
    public async Task<ResultModel<ReportDto>> CreateReportAsync(
        Guid reporterId, CreateReportRequest request, CancellationToken ct = default)
    {
        // Cannot report own content
        if (request.TargetType == ReportTargetType.User && request.TargetId == reporterId)
            return ResultModel<ReportDto>.BadRequest("You cannot report yourself.");

        // One report per target per user
        var duplicate = await uow.Reports.Query()
            .AnyAsync(r => r.ReporterId == reporterId
                        && r.TargetType == request.TargetType
                        && r.TargetId == request.TargetId, ct);

        if (duplicate)
            return ResultModel<ReportDto>.Conflict("You have already reported this content.");

        var report = new Report
        {
            ReporterId  = reporterId,
            TargetType  = request.TargetType,
            TargetId    = request.TargetId,
            Reason      = request.Reason,
            Description = request.Description,
            Status      = ReportStatus.Pending
        };

        await uow.Reports.AddAsync(report, ct);
        await uow.SaveChangesAsync(ct);

        // Auto-flag: when a target reaches 5 reports, move all pending to UnderReview
        const int AutoFlagThreshold = 5;
        var pendingCount = await uow.Reports.Query()
            .CountAsync(r => r.TargetType == request.TargetType
                          && r.TargetId   == request.TargetId
                          && r.Status     == ReportStatus.Pending, ct);

        if (pendingCount >= AutoFlagThreshold)
        {
            var pendingReports = await uow.Reports.Query()
                .Where(r => r.TargetType == request.TargetType
                         && r.TargetId   == request.TargetId
                         && r.Status     == ReportStatus.Pending)
                .ToListAsync(ct);

            foreach (var pr in pendingReports)
            {
                pr.Status = ReportStatus.UnderReview;
                uow.Reports.Update(pr);
            }

            await uow.SaveChangesAsync(ct);
            logger.LogWarning("Auto-flagged {TargetType} {TargetId} for review after {Count} reports",
                request.TargetType, request.TargetId, pendingCount);
        }

        logger.LogInformation("User {ReporterId} reported {TargetType} {TargetId}", reporterId, request.TargetType, request.TargetId);

        var created = await LoadReport(report.Id, ct);
        return ResultModel<ReportDto>.Created(MapToDto(created!));
    }

    public async Task<ResultModel<IReadOnlyList<ReportDto>>> GetMyReportsAsync(
        Guid userId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var reports = await uow.Reports.Query()
            .Include(r => r.Reporter)
            .Where(r => r.ReporterId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return ResultModel<IReadOnlyList<ReportDto>>.Ok(reports.Select(MapToDto).ToList());
    }

    public async Task<ResultModel<PagedResult<ReportDto>>> GetReportsAsync(
        string? status, string? targetType, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var query = uow.Reports.Query().Include(r => r.Reporter).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReportStatus>(status, true, out var parsedStatus))
            query = query.Where(r => r.Status == parsedStatus);

        if (!string.IsNullOrWhiteSpace(targetType) && Enum.TryParse<ReportTargetType>(targetType, true, out var parsedTarget))
            query = query.Where(r => r.TargetType == parsedTarget);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return ResultModel<PagedResult<ReportDto>>.Ok(new PagedResult<ReportDto>
        {
            Items      = items.Select(MapToDto).ToList(),
            Total      = total,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        });
    }

    public async Task<ResultModel<ReportDto>> GetReportByIdAsync(Guid reportId, CancellationToken ct = default)
    {
        var report = await LoadReport(reportId, ct);
        if (report is null)
            return ResultModel<ReportDto>.NotFound("Report not found.");

        return ResultModel<ReportDto>.Ok(MapToDto(report));
    }

    public async Task<ResultModel> ReviewReportAsync(
        Guid adminId, Guid reportId, ReviewReportRequest request, CancellationToken ct = default)
    {
        var report = await uow.Reports.GetByIdAsync(reportId, ct);
        if (report is null)
            return ResultModel.NotFound("Report not found.");

        if (report.Status is ReportStatus.Resolved or ReportStatus.Rejected)
            return ResultModel.BadRequest("Report has already been reviewed.");

        if (!Enum.TryParse<ReportStatus>(request.Decision, true, out var decision)
            || decision is not (ReportStatus.Resolved or ReportStatus.Rejected))
            return ResultModel.BadRequest("Decision must be 'Resolved' or 'Rejected'.");

        report.Status             = decision;
        report.ReviewedByAdminId  = adminId;
        report.AdminNote          = request.Note;
        report.ResolvedAt         = DateTime.UtcNow;

        uow.Reports.Update(report);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Admin {AdminId} reviewed report {ReportId} as {Decision}", adminId, reportId, decision);
        return ResultModel.NoContent();
    }

    private async Task<Report?> LoadReport(Guid id, CancellationToken ct)
        => await uow.Reports.Query()
            .Include(r => r.Reporter)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    private static ReportDto MapToDto(Report r) => new()
    {
        Id               = r.Id,
        ReporterId       = r.ReporterId,
        ReporterUsername = r.Reporter.UserName ?? string.Empty,
        TargetType       = r.TargetType.ToString(),
        TargetId         = r.TargetId,
        Reason           = r.Reason.ToString(),
        Description      = r.Description,
        Status           = r.Status.ToString(),
        AdminNote        = r.AdminNote,
        ResolvedAt       = r.ResolvedAt,
        CreatedAt        = r.CreatedAt
    };
}
