export interface SocialLinkDto {
  id?: string;
  platform: string;
  url: string;
}

export interface BlogThemeDto {
  primaryColor: string | null;
  secondaryColor: string | null;
  accentColor: string | null;
  fontFamily: string | null;
  layoutStyle: string | null;
  darkModeDefault: boolean;
}

export interface BlogNoticeDto {
  id: string;
  title: string;
  content: string;
  type: string;
  isActive: boolean;
  expiresAt: string | null;
  priority: number;
  createdAt: string;
}

export interface QuickLinkDto {
  id: string;
  title: string;
  url: string;
  icon: string | null;
  order: number;
}

export interface UserWordFilterDto {
  id: string;
  word: string;
  createdAt: string;
}

export interface BlogProfileDto {
  userId: string;
  username: string;
  displayName: string;
  bio: string | null;
  avatarUrl: string | null;
  slug: string;
  about: string | null;
  logoUrl: string | null;
  bannerUrl: string | null;
  tagline: string | null;
  theme: BlogThemeDto | null;
  websiteUrl: string | null;
  socialLinks: SocialLinkDto[];
  activeNotices: BlogNoticeDto[];
  quickLinks: QuickLinkDto[];
  followersCount: number;
  followingCount: number;
  postsCount: number;
  isFollowing: boolean;
  isBlocked: boolean;
  createdAt: string;
}

export interface UpdateProfileRequest {
  displayName?: string;
  bio?: string;
  about?: string;
  logoUrl?: string;
  bannerUrl?: string;
  tagline?: string;
  websiteUrl?: string;
  socialLinks?: SocialLinkDto[];
}

export interface UpdateBlogThemeRequest {
  primaryColor?: string | null;
  secondaryColor?: string | null;
  accentColor?: string | null;
  fontFamily?: string | null;
  layoutStyle?: string | null;
  darkModeDefault?: boolean;
}

export interface CreateBlogNoticeRequest {
  title: string;
  content: string;
  type: string;
  isActive: boolean;
  expiresAt?: string | null;
  priority: number;
}

export interface CreateQuickLinkRequest {
  title: string;
  url: string;
  icon?: string | null;
  order: number;
}

export interface AddWordFilterRequest {
  word: string;
}

export interface UserPreferencesDto {
  preferredLanguage: string;
  receiveEmailNotifications: boolean;
  profileVisibility: string;
}

export interface UpdatePreferencesRequest {
  receiveEmailNotifications: boolean;
  profileVisibility: string;
}
