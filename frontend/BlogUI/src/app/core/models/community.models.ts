export interface CommentDto {
  id: string;
  postId: string;
  parentId: string | null;
  authorId: string;
  authorUsername: string;
  authorDisplayName: string;
  authorAvatarUrl: string | null;
  body: string;
  likesCount: number;
  isDeleted: boolean;
  createdAt: string;
  replies: CommentDto[];
}

export interface CreateCommentRequest {
  body: string;
  parentId?: string;
}

export interface UpdateCommentRequest {
  body: string;
}

export interface NotificationDto {
  id: string;
  type: string;
  actorId: string;
  actorUsername: string;
  actorDisplayName: string;
  actorAvatarUrl: string | null;
  postId: string | null;
  postSlug: string | null;
  postTitle: string | null;
  commentId: string | null;
  isRead: boolean;
  createdAt: string;
}

export interface FollowerDto {
  userId: string;
  username: string;
  displayName: string;
  avatarUrl: string | null;
  profileSlug: string;
}

export type ReportTargetType = 'User' | 'Post' | 'Comment';
export type ReportReason = 'Spam' | 'Harassment' | 'HateSpeech' | 'FakeAccount' | 'AdultContent' | 'Copyright' | 'Other';

export interface CreateReportRequest {
  targetType: ReportTargetType;
  targetId: string;
  reason: ReportReason;
  description?: string;
}

export interface ReportDto {
  id: string;
  reporterId: string;
  reporterUsername: string;
  targetType: string;
  targetId: string;
  reason: string;
  description?: string;
  status: string;
  adminNote?: string;
  resolvedAt?: string;
  createdAt: string;
}

export interface BlockedUserDto {
  userId: string;
  username: string;
  displayName: string | null;
  avatarUrl: string | null;
  blockedAt: string;
}
