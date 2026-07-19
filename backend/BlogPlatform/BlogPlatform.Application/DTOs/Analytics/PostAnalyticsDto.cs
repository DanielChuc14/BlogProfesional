namespace BlogPlatform.Application.DTOs.Analytics;

public class PostAnalyticsDto
{
    public Guid PostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TotalViews { get; set; }
    public int UniqueVisitors { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public Dictionary<string, int> ViewsByDay { get; set; } = new();
    public Dictionary<string, int> ViewsByDevice { get; set; } = new();
    public Dictionary<string, int> TopReferrers { get; set; } = new();
}
