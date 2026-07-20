export type PostStatus = 'Draft' | 'Published' | 'Scheduled' | 'Archived';

export interface PostAuthorDto {
  userId: string;
  username: string;
  displayName: string;
  avatarUrl: string | null;
  profileSlug: string;
}

export interface PostTagDto {
  id: string;
  name: string;
  slug: string;
}

export interface PostSeoDto {
  metaTitle: string | null;
  metaDescription: string | null;
  canonicalUrl: string | null;
}

export interface PostDetailDto {
  id: string;
  title: string;
  slug: string;
  excerpt: string | null;
  body: string;
  coverImageUrl: string | null;
  status: PostStatus;
  author: PostAuthorDto;
  tags: PostTagDto[];
  seo: PostSeoDto | null;
  viewCount: number;
  likesCount: number;
  commentsCount: number;
  likedByCurrentUser: boolean;
  publishedAt: string | null;
  createdAt: string;
  updatedAt: string;
  isAdultContent: boolean;
}

export interface PostSummaryDto {
  id: string;
  title: string;
  slug: string;
  excerpt: string | null;
  coverImageUrl: string | null;
  status: PostStatus;
  author: PostAuthorDto;
  tags: PostTagDto[];
  viewCount: number;
  likesCount: number;
  commentsCount: number;
  publishedAt: string | null;
  createdAt: string;
  readTimeMinutes: number | null;
  isAdultContent: boolean;
  isFeatured: boolean;
}

export interface CursorPageResult<T> {
  items: T[];
  nextCursor: string | null;
  hasMore: boolean;
}

export interface PostFeedQuery {
  cursor?: string;
  pageSize?: number;
  tag?: string;
  author?: string;
}

export interface CreatePostRequest {
  title: string;
  body: string;
  excerpt?: string;
  coverImageUrl?: string;
  tagIds?: string[];
  isAdultContent?: boolean;
  seo?: {
    metaTitle?: string;
    metaDescription?: string;
    canonicalUrl?: string;
  };
}

export interface UpdatePostRequest extends CreatePostRequest {}

export interface SchedulePostRequest {
  scheduledAt: string;
}
