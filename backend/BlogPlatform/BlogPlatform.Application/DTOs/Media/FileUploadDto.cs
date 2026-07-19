namespace BlogPlatform.Application.DTOs.Media;

public record FileUploadDto(
    Stream Stream,
    string FileName,
    string ContentType,
    long Size);
