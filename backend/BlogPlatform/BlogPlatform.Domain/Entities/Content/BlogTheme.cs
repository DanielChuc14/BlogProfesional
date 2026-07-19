using System.Text.Json;

namespace BlogPlatform.Domain.Entities.Content;

public class BlogTheme : Common.BaseEntity
{
    public Guid BlogProfileId { get; set; }
    public JsonDocument Config { get; set; } = JsonDocument.Parse("{}");

    // Navigation
    public BlogProfile BlogProfile { get; set; } = null!;
}
