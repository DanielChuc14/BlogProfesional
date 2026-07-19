using BlogPlatform.Application.DTOs.Admin;
using BlogPlatform.Application.Interfaces;
using BlogPlatform.Domain.Entities.Admin;
using BlogPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace BlogPlatform.Infrastructure.Services;

public class PlatformSettingsService(AppDbContext db, IDistributedCache cache) : IPlatformSettingsService
{
    private const string CachePrefix = "platform_setting:";
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    };

    public async Task<string?> GetValueAsync(string key, CancellationToken ct = default)
    {
        var cacheKey = CachePrefix + key;
        var cached = await cache.GetStringAsync(cacheKey, ct);
        if (cached is not null) return cached;

        var setting = await db.PlatformSettings.FindAsync([key], ct);
        if (setting is null) return null;

        await cache.SetStringAsync(cacheKey, setting.Value, CacheOptions, ct);
        return setting.Value;
    }

    public async Task<List<PlatformSettingDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.PlatformSettings
            .OrderBy(p => p.Key)
            .Select(p => new PlatformSettingDto
            {
                Key = p.Key,
                Value = p.Value,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync(ct);
    }

    public async Task UpsertManyAsync(Dictionary<string, string> settings, Guid updatedByUserId, CancellationToken ct = default)
    {
        foreach (var (key, value) in settings)
        {
            var existing = await db.PlatformSettings.FindAsync([key], ct);
            if (existing is null)
            {
                db.PlatformSettings.Add(new PlatformSetting
                {
                    Key = key,
                    Value = value,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedByUserId = updatedByUserId
                });
            }
            else
            {
                existing.Value = value;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedByUserId = updatedByUserId;
                db.PlatformSettings.Update(existing);
            }

            await cache.RemoveAsync(CachePrefix + key, ct);
        }

        await db.SaveChangesAsync(ct);
    }
}
