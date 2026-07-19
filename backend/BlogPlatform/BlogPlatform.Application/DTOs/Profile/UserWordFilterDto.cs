namespace BlogPlatform.Application.DTOs.Profile;

public class UserWordFilterDto
{
    public Guid Id { get; init; }
    public string Word { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public class AddWordFilterRequest
{
    public string Word { get; init; } = string.Empty;
}
