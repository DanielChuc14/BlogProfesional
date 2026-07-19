namespace BlogPlatform.Application.DTOs.Analytics;

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int TotalPosts { get; set; }
    public int TotalComments { get; set; }
    public int TotalPageViews { get; set; }
    public int NewUsersLast30Days { get; set; }
    public int NewPostsLast30Days { get; set; }
    public int PageViewsLast30Days { get; set; }
    public int ActiveBloggers { get; set; }
}
