using BlogPlatform.Application.DTOs.Media;
using BlogPlatform.Domain.Common;

namespace BlogPlatform.Application.Interfaces;

public interface IMediaService
{
    Task<ResultModel<MediaUploadResponse>> UploadAsync(Guid userId, FileUploadDto file, CancellationToken ct = default);
}
