using System.Text.Json;
using BlogPlatform.Application.DTOs.Admin;
using BlogPlatform.Application.Interfaces;
using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Entities.Admin;
using BlogPlatform.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Infrastructure.Services;

public class LanguageService(
    IUnitOfWork uow,
    IAuditService auditService) : ILanguageService
{
    // ── Public ────────────────────────────────────────────────────────────────

    public async Task<ResultModel<List<LanguageDto>>> GetActiveLanguagesAsync(CancellationToken ct = default)
    {
        var languages = await uow.Languages.Query()
            .Where(l => l.IsActive)
            .OrderBy(l => l.Code)
            .Select(l => new LanguageDto
            {
                Code = l.Code,
                Name = l.Name,
                NativeName = l.NativeName,
            })
            .ToListAsync(ct);

        return ResultModel<List<LanguageDto>>.Ok(languages);
    }

    public async Task<ResultModel<string>> GetTranslationAsync(string code, CancellationToken ct = default)
    {
        var language = await uow.Languages.Query()
            .FirstOrDefaultAsync(l => l.Code == code && l.IsActive, ct);

        if (language is null)
            return ResultModel<string>.NotFound($"Language '{code}' not found or inactive.");

        if (string.IsNullOrWhiteSpace(language.TranslationJson))
            return ResultModel<string>.NotFound($"No translation file uploaded for language '{code}'.");

        return ResultModel<string>.Ok(language.TranslationJson);
    }

    // ── Admin ─────────────────────────────────────────────────────────────────

    public async Task<ResultModel<List<AdminLanguageDto>>> GetAllLanguagesAsync(CancellationToken ct = default)
    {
        var languages = await uow.Languages.Query()
            .OrderByDescending(l => l.IsDefault)
            .ThenBy(l => l.Code)
            .Select(l => new AdminLanguageDto
            {
                Id = l.Id,
                Code = l.Code,
                Name = l.Name,
                NativeName = l.NativeName,
                IsActive = l.IsActive,
                IsDefault = l.IsDefault,
                HasTranslation = l.TranslationJson != null,
                CreatedAt = l.CreatedAt,
            })
            .ToListAsync(ct);

        return ResultModel<List<AdminLanguageDto>>.Ok(languages);
    }

    public async Task<ResultModel<AdminLanguageDto>> CreateLanguageAsync(
        CreateLanguageRequest request,
        CancellationToken ct = default)
    {
        var code = request.Code.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(code) || code.Length > 10)
            return ResultModel<AdminLanguageDto>.BadRequest("Language code must be between 1 and 10 characters.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return ResultModel<AdminLanguageDto>.BadRequest("Language name is required.");

        if (string.IsNullOrWhiteSpace(request.NativeName))
            return ResultModel<AdminLanguageDto>.BadRequest("Native name is required.");

        var exists = await uow.Languages.Query().AnyAsync(l => l.Code == code, ct);
        if (exists)
            return ResultModel<AdminLanguageDto>.Conflict($"Language '{code}' already exists.");

        var language = new Language
        {
            Code = code,
            Name = request.Name.Trim(),
            NativeName = request.NativeName.Trim(),
            IsActive = request.IsActive,
            IsDefault = false,
        };

        await uow.Languages.AddAsync(language, ct);
        await uow.SaveChangesAsync(ct);

        await auditService.LogAsync("CreateLanguage", "Language", code, ct: ct);

        return ResultModel<AdminLanguageDto>.Created(ToAdminDto(language));
    }

    public async Task<ResultModel<AdminLanguageDto>> UpdateLanguageAsync(
        string code,
        UpdateLanguageAdminRequest request,
        CancellationToken ct = default)
    {
        var language = await uow.Languages.Query()
            .FirstOrDefaultAsync(l => l.Code == code, ct);

        if (language is null)
            return ResultModel<AdminLanguageDto>.NotFound($"Language '{code}' not found.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return ResultModel<AdminLanguageDto>.BadRequest("Language name is required.");

        if (string.IsNullOrWhiteSpace(request.NativeName))
            return ResultModel<AdminLanguageDto>.BadRequest("Native name is required.");

        language.Name = request.Name.Trim();
        language.NativeName = request.NativeName.Trim();
        language.UpdatedAt = DateTime.UtcNow;

        uow.Languages.Update(language);
        await uow.SaveChangesAsync(ct);

        await auditService.LogAsync("UpdateLanguage", "Language", code, ct: ct);

        return ResultModel<AdminLanguageDto>.Ok(ToAdminDto(language));
    }

    public async Task<ResultModel> UploadTranslationAsync(
        string code,
        string jsonContent,
        CancellationToken ct = default)
    {
        var language = await uow.Languages.Query()
            .FirstOrDefaultAsync(l => l.Code == code, ct);

        if (language is null)
            return ResultModel.NotFound($"Language '{code}' not found.");

        try
        {
            JsonDocument.Parse(jsonContent);
        }
        catch (JsonException)
        {
            return ResultModel.BadRequest("The uploaded file is not valid JSON.");
        }

        language.TranslationJson = jsonContent;
        language.UpdatedAt = DateTime.UtcNow;

        uow.Languages.Update(language);
        await uow.SaveChangesAsync(ct);

        await auditService.LogAsync("UploadTranslation", "Language", code, ct: ct);

        return ResultModel.NoContent();
    }

    public async Task<ResultModel> ToggleLanguageAsync(string code, CancellationToken ct = default)
    {
        var language = await uow.Languages.Query()
            .FirstOrDefaultAsync(l => l.Code == code, ct);

        if (language is null)
            return ResultModel.NotFound($"Language '{code}' not found.");

        if (language.IsDefault && language.IsActive)
            return ResultModel.BadRequest("Cannot deactivate the default language.");

        language.IsActive = !language.IsActive;
        language.UpdatedAt = DateTime.UtcNow;

        uow.Languages.Update(language);
        await uow.SaveChangesAsync(ct);

        await auditService.LogAsync(
            language.IsActive ? "ActivateLanguage" : "DeactivateLanguage",
            "Language", code, ct: ct);

        return ResultModel.NoContent();
    }

    public async Task<ResultModel> DeleteLanguageAsync(string code, CancellationToken ct = default)
    {
        var language = await uow.Languages.Query()
            .FirstOrDefaultAsync(l => l.Code == code, ct);

        if (language is null)
            return ResultModel.NotFound($"Language '{code}' not found.");

        if (language.IsDefault)
            return ResultModel.BadRequest("Cannot delete the default language.");

        uow.Languages.Remove(language);
        await uow.SaveChangesAsync(ct);

        await auditService.LogAsync("DeleteLanguage", "Language", code, ct: ct);

        return ResultModel.NoContent();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static AdminLanguageDto ToAdminDto(Language l) => new()
    {
        Id = l.Id,
        Code = l.Code,
        Name = l.Name,
        NativeName = l.NativeName,
        IsActive = l.IsActive,
        IsDefault = l.IsDefault,
        HasTranslation = l.TranslationJson != null,
        CreatedAt = l.CreatedAt,
    };
}
