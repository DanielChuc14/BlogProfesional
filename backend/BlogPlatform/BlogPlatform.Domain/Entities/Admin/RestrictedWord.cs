using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Enums;

namespace BlogPlatform.Domain.Entities.Admin;

public class RestrictedWord : BaseEntity
{
    public string Phrase { get; set; } = string.Empty;
    public bool IsRegex { get; set; } = false;
    public RestrictedWordSeverity Severity { get; set; } = RestrictedWordSeverity.Block;
    public Guid AddedByUserId { get; set; }
}
