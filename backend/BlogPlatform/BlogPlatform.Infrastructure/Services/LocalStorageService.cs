using BlogPlatform.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Infrastructure.Services;

public class LocalStorageService(
    IConfiguration configuration,
    ILogger<LocalStorageService> logger) : IStorageService
{
    private readonly string _basePath = configuration["Storage:BasePath"] ?? "/uploads";
    private readonly string _baseUrl = configuration["Storage:BaseUrl"] ?? "http://localhost:5000/uploads";

    public async Task<string> SaveAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        var sanitized = Path.GetFileName(fileName);
        var unique = $"{Guid.NewGuid():N}_{sanitized}";
        var fullPath = Path.Combine(_basePath, unique);

        Directory.CreateDirectory(_basePath);

        await using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await stream.CopyToAsync(fs, ct);

        logger.LogInformation("File saved: {Path}", fullPath);

        return $"{_baseUrl.TrimEnd('/')}/{unique}";
    }

    public Task DeleteAsync(string filePath, CancellationToken ct = default)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            logger.LogInformation("File deleted: {Path}", filePath);
        }

        return Task.CompletedTask;
    }

    public bool Exists(string filePath) => File.Exists(filePath);
}
