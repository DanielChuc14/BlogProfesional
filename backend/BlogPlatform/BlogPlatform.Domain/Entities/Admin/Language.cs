using BlogPlatform.Domain.Common;

namespace BlogPlatform.Domain.Entities.Admin;

public class Language : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    public string? TranslationJson { get; set; }
}
