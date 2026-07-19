using BlogPlatform.Application.DTOs.Media;
using BlogPlatform.Application.Interfaces;
using BlogPlatform.Domain.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Infrastructure.Services;

public class MediaService(
    IStorageService storage,
    IConfiguration configuration,
    ILogger<MediaService> logger) : IMediaService
{
    public async Task<ResultModel<MediaUploadResponse>> UploadAsync(Guid userId, FileUploadDto file, CancellationToken ct = default)
    {
        var contentTypeLower = file.ContentType.ToLower();

        var isImage = contentTypeLower.StartsWith("image/");
        var isPdf = contentTypeLower == "application/pdf";

        if (!isImage && !isPdf)
            return ResultModel<MediaUploadResponse>.BadRequest("Only images and PDF files are allowed.");

        if (isImage)
        {
            var maxMb = int.TryParse(configuration["Storage:MaxImageMB"], out var mb) ? mb : 5;
            if (file.Size > maxMb * 1024L * 1024L)
                return ResultModel<MediaUploadResponse>.BadRequest($"Image must not exceed {maxMb} MB.");
        }

        if (isPdf)
        {
            var maxMb = int.TryParse(configuration["Storage:MaxPdfMB"], out var mb) ? mb : 20;
            if (file.Size > maxMb * 1024L * 1024L)
                return ResultModel<MediaUploadResponse>.BadRequest($"PDF must not exceed {maxMb} MB.");
        }

        var url = await storage.SaveAsync(file.Stream, file.FileName, file.ContentType, ct);

        logger.LogInformation("Media uploaded by user {UserId}: {Url}", userId, url);

        return ResultModel<MediaUploadResponse>.Created(new MediaUploadResponse
        {
            Url = url,
            FileName = file.FileName,
            ContentType = file.ContentType,
            Size = file.Size
        });
    }
}
