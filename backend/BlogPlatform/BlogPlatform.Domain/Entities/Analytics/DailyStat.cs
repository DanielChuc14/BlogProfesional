using BlogPlatform.Domain.Entities.Content;

namespace BlogPlatform.Domain.Entities.Analytics;

public class DailyStat
{
    public Guid Id { get; set; }
    public Guid BlogProfileId { get; set; }
    public DateOnly Date { get; set; }
    public int ViewCount { get; set; }
    public int UniqueVisitors { get; set; }
    public int NewFollowers { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }

    // Navigation
    public BlogProfile BlogProfile { get; set; } = null!;
}
