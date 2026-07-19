using BlogPlatform.Application.DTOs.Admin;
using BlogPlatform.Domain.Common;

namespace BlogPlatform.Application.Interfaces;

public interface ILanguageService
{
    // Public
    Task<ResultModel<List<LanguageDto>>> GetActiveLanguagesAsync(CancellationToken ct = default);
    Task<ResultModel<string>> GetTranslationAsync(string code, CancellationToken ct = default);

    // Admin
    Task<ResultModel<List<AdminLanguageDto>>> GetAllLanguagesAsync(CancellationToken ct = default);
    Task<ResultModel<AdminLanguageDto>> CreateLanguageAsync(CreateLanguageRequest request, CancellationToken ct = default);
    Task<ResultModel<AdminLanguageDto>> UpdateLanguageAsync(string code, UpdateLanguageAdminRequest request, CancellationToken ct = default);
    Task<ResultModel> UploadTranslationAsync(string code, string jsonContent, CancellationToken ct = default);
    Task<ResultModel> ToggleLanguageAsync(string code, CancellationToken ct = default);
    Task<ResultModel> DeleteLanguageAsync(string code, CancellationToken ct = default);
}
