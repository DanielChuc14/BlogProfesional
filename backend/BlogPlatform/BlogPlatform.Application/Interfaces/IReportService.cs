using BlogPlatform.Application.DTOs.Common;
using BlogPlatform.Application.DTOs.Security;
using BlogPlatform.Domain.Common;

namespace BlogPlatform.Application.Interfaces;

public interface IReportService
{
    Task<ResultModel<ReportDto>> CreateReportAsync(Guid reporterId, CreateReportRequest request, CancellationToken ct = default);
    Task<ResultModel<IReadOnlyList<ReportDto>>> GetMyReportsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<ResultModel<PagedResult<ReportDto>>> GetReportsAsync(string? status, string? targetType, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<ResultModel<ReportDto>> GetReportByIdAsync(Guid reportId, CancellationToken ct = default);
    Task<ResultModel> ReviewReportAsync(Guid adminId, Guid reportId, ReviewReportRequest request, CancellationToken ct = default);
}
