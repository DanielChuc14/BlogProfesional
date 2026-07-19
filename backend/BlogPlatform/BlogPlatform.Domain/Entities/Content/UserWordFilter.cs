using BlogPlatform.Domain.Common;

namespace BlogPlatform.Domain.Entities.Content;

public class UserWordFilter : BaseEntity
{
    public Guid UserId { get; set; }
    public string Word { get; set; } = string.Empty;
}
