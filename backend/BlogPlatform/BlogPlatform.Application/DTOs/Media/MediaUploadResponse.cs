namespace BlogPlatform.Application.DTOs.Media;

public class MediaUploadResponse
{
    public string Url { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Size { get; init; }
}
