namespace BlogPlatform.Application.DTOs.Common;

public class CursorPagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public string? NextCursor { get; init; }
    public bool HasMore { get; init; }
}
