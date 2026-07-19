namespace BlogPlatform.Application.DTOs.Analytics;

public class DailyStatDto
{
    public DateOnly Date { get; set; }
    public int ViewCount { get; set; }
    public int UniqueVisitors { get; set; }
    public int NewFollowers { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
}
