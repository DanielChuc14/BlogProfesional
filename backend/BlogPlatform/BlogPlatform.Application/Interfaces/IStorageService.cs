namespace BlogPlatform.Application.Interfaces;

public interface IStorageService
{
    Task<string> SaveAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default);
    Task DeleteAsync(string filePath, CancellationToken ct = default);
    bool Exists(string filePath);
}
