namespace BlogPlatform.Application.DTOs.Analytics;

public class BloggerDashboardDto
{
    public int TotalPosts { get; set; }
    public int TotalViews { get; set; }
    public int TotalLikes { get; set; }
    public int TotalComments { get; set; }
    public int TotalFollowers { get; set; }
    public int ViewsLast30Days { get; set; }
    public int LikesLast30Days { get; set; }
    public int CommentsLast30Days { get; set; }
    public IReadOnlyList<DailyStatDto> DailyStats { get; set; } = [];
    public IReadOnlyList<TopPostDto> TopPosts { get; set; } = [];
}
