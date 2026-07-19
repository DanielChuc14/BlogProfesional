export interface DailyStatDto {
  date: string;
  viewCount: number;
  uniqueVisitors: number;
  newFollowers: number;
  likesCount: number;
  commentsCount: number;
}

export interface TopPostDto {
  id: string;
  title: string;
  slug: string;
  viewCount: number;
  likesCount: number;
  commentsCount: number;
  publishedAt: string | null;
}

export interface BloggerDashboardDto {
  totalPosts: number;
  totalViews: number;
  totalLikes: number;
  totalComments: number;
  totalFollowers: number;
  viewsLast30Days: number;
  likesLast30Days: number;
  commentsLast30Days: number;
  dailyStats: DailyStatDto[];
  topPosts: TopPostDto[];
}

export interface PostAnalyticsDto {
  postId: string;
  title: string;
  totalViews: number;
  uniqueVisitors: number;
  likesCount: number;
  commentsCount: number;
  viewsByDay: Record<string, number>;
  viewsByDevice: Record<string, number>;
  topReferrers: Record<string, number>;
}

export interface AdminDashboardDto {
  totalUsers: number;
  totalPosts: number;
  totalComments: number;
  totalPageViews: number;
  newUsersLast30Days: number;
  newPostsLast30Days: number;
  pageViewsLast30Days: number;
  activeBloggers: number;
}
