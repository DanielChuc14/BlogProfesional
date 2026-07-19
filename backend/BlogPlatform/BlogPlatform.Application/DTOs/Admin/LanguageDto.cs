namespace BlogPlatform.Application.DTOs.Admin;

public class LanguageDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string NativeName { get; init; } = string.Empty;
}

public class AdminLanguageDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string NativeName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool IsDefault { get; init; }
    public bool HasTranslation { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class CreateLanguageRequest
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string NativeName { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
}

public class UpdateLanguageAdminRequest
{
    public string Name { get; init; } = string.Empty;
    public string NativeName { get; init; } = string.Empty;
}
