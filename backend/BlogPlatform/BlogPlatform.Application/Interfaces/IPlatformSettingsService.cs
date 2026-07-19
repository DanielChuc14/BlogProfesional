using BlogPlatform.Application.DTOs.Admin;

namespace BlogPlatform.Application.Interfaces;

public interface IPlatformSettingsService
{
    Task<string?> GetValueAsync(string key, CancellationToken ct = default);
    Task<List<PlatformSettingDto>> GetAllAsync(CancellationToken ct = default);
    Task UpsertManyAsync(Dictionary<string, string> settings, Guid updatedByUserId, CancellationToken ct = default);
}
