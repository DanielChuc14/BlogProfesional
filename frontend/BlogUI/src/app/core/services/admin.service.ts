import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { PagedResult, TagDto } from '../models';

export interface UserSummary {
  id: string;
  username: string;
  displayName: string;
  email: string;
  isActive: boolean;
  emailConfirmed: boolean;
  roles: string[];
  profileSlug?: string;
  suspendedUntil?: string;
  createdAt: string;
}

export interface UserSuspensionDto {
  id: string;
  reason: string;
  expiresAt: string;
  isActive: boolean;
  suspendedByUsername: string;
  liftedByUsername?: string;
  liftedAt?: string;
  createdAt: string;
}

export interface SuspendUserRequest {
  reason: string;
  durationDays: number;
}

export interface RoleDto {
  name: string;
  userCount: number;
}

export interface CommentSummary {
  id: string;
  body: string;
  authorUsername: string;
  postTitle: string;
  postSlug: string;
  isDeleted: boolean;
  likesCount: number;
  createdAt: string;
}

export interface AdminPostSummary {
  id: string;
  title: string;
  slug: string;
  status: string;
  authorUsername: string;
  viewCount: number;
  likesCount: number;
  commentsCount: number;
  publishedAt?: string;
  createdAt: string;
}

export interface RestrictedWordDto {
  id: string;
  phrase: string;
  isRegex: boolean;
  severity: 'Warn' | 'Block';
  createdAt: string;
}

export interface AuditLogDto {
  id: string;
  actorId: string;
  actorUsername: string;
  action: string;
  entityType: string;
  entityId?: string;
  reason?: string;
  createdAt: string;
}

export interface AdminLanguageDto {
  id: string;
  code: string;
  name: string;
  nativeName: string;
  isActive: boolean;
  isDefault: boolean;
  hasTranslation: boolean;
  createdAt: string;
}

export interface CreateLanguageRequest {
  code: string;
  name: string;
  nativeName: string;
  isActive: boolean;
}

export interface UpdateLanguageRequest {
  name: string;
  nativeName: string;
}

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly api = inject(ApiService);

  // Users
  getUsers(page = 1, pageSize = 20, search?: string): Observable<PagedResult<UserSummary>> {
    return this.api.get<PagedResult<UserSummary>>('/api/admin/users', { page, pageSize, search });
  }

  getUserById(id: string): Observable<UserSummary> {
    return this.api.get<UserSummary>(`/api/admin/users/${id}`);
  }

  deleteUser(id: string): Observable<void> {
    return this.api.delete<void>(`/api/admin/users/${id}`);
  }

  banUser(id: string): Observable<void> {
    return this.api.patch<void>(`/api/admin/users/${id}/ban`);
  }

  unbanUser(id: string): Observable<void> {
    return this.api.patch<void>(`/api/admin/users/${id}/unban`);
  }

  suspendUser(id: string, req: SuspendUserRequest): Observable<void> {
    return this.api.post<void>(`/api/admin/users/${id}/suspend`, req);
  }

  liftSuspension(id: string): Observable<void> {
    return this.api.delete<void>(`/api/admin/users/${id}/suspend`);
  }

  getSuspensionHistory(id: string): Observable<UserSuspensionDto[]> {
    return this.api.get<UserSuspensionDto[]>(`/api/admin/users/${id}/suspensions`);
  }

  changeRole(userId: string, role: string): Observable<void> {
    return this.api.put<void>(`/api/admin/users/${userId}/role`, { role });
  }

  assignRole(userId: string, role: string): Observable<void> {
    return this.api.post<void>(`/api/admin/users/${userId}/roles`, { role });
  }

  removeRole(userId: string, role: string): Observable<void> {
    return this.api.delete<void>(`/api/admin/users/${userId}/roles/${role}`);
  }

  // Roles
  getRoles(): Observable<RoleDto[]> {
    return this.api.get<RoleDto[]>('/api/admin/roles');
  }

  createRole(name: string): Observable<RoleDto> {
    return this.api.post<RoleDto>('/api/admin/roles', { role: name });
  }

  deleteRole(name: string): Observable<void> {
    return this.api.delete<void>(`/api/admin/roles/${name}`);
  }

  // Posts
  getAdminPosts(page = 1, pageSize = 20, status?: string): Observable<PagedResult<AdminPostSummary>> {
    return this.api.get<PagedResult<AdminPostSummary>>('/api/admin/posts', { page, pageSize, status });
  }

  unpublishPost(id: string, reason: string): Observable<void> {
    return this.api.patch<void>(`/api/admin/posts/${id}/unpublish`, { reason });
  }

  forceDeletePost(id: string): Observable<void> {
    return this.api.delete<void>(`/api/admin/posts/${id}`);
  }

  // Comments (moderation)
  getAdminComments(page = 1, pageSize = 20): Observable<PagedResult<CommentSummary>> {
    return this.api.get<PagedResult<CommentSummary>>('/api/admin/comments', { page, pageSize });
  }

  deleteComment(id: string): Observable<void> {
    return this.api.delete<void>(`/api/admin/comments/${id}`);
  }

  // Settings
  getSettings(): Observable<{ key: string; value: string; updatedAt: string }[]> {
    return this.api.get<{ key: string; value: string; updatedAt: string }[]>('/api/admin/settings');
  }

  updateSettings(settings: Record<string, string>): Observable<void> {
    return this.api.put<void>('/api/admin/settings', { settings });
  }

  // Tags
  getAdminTags(): Observable<TagDto[]> {
    return this.api.get<TagDto[]>('/api/admin/tags');
  }

  createAdminTag(name: string): Observable<TagDto> {
    return this.api.post<TagDto>('/api/admin/tags', { name });
  }

  updateAdminTag(id: string, name: string): Observable<TagDto> {
    return this.api.put<TagDto>(`/api/admin/tags/${id}`, { name });
  }

  deleteTag(id: string): Observable<void> {
    return this.api.delete<void>(`/api/admin/tags/${id}`);
  }

  // Restricted Words
  getRestrictedWords(): Observable<RestrictedWordDto[]> {
    return this.api.get<RestrictedWordDto[]>('/api/admin/restricted-words');
  }

  addRestrictedWord(phrase: string, isRegex: boolean = false, severity: 'Warn' | 'Block' = 'Block'): Observable<RestrictedWordDto> {
    return this.api.post<RestrictedWordDto>('/api/admin/restricted-words', { phrase, isRegex, severity });
  }

  getAuditLogs(page = 1, pageSize = 30): Observable<PagedResult<AuditLogDto>> {
    return this.api.get<PagedResult<AuditLogDto>>('/api/admin/audit-logs', { page, pageSize });
  }

  deleteRestrictedWord(id: string): Observable<void> {
    return this.api.delete<void>(`/api/admin/restricted-words/${id}`);
  }

  // Languages
  getAdminLanguages(): Observable<AdminLanguageDto[]> {
    return this.api.get<AdminLanguageDto[]>('/api/admin/languages');
  }

  createLanguage(req: CreateLanguageRequest): Observable<AdminLanguageDto> {
    return this.api.post<AdminLanguageDto>('/api/admin/languages', req);
  }

  updateLanguage(code: string, req: UpdateLanguageRequest): Observable<AdminLanguageDto> {
    return this.api.put<AdminLanguageDto>(`/api/admin/languages/${code}`, req);
  }

  uploadTranslation(code: string, file: File): Observable<void> {
    const form = new FormData();
    form.append('file', file);
    return this.api.postForm<void>(`/api/admin/languages/${code}/translations`, form);
  }

  toggleLanguage(code: string): Observable<void> {
    return this.api.patch<void>(`/api/admin/languages/${code}/toggle`);
  }

  deleteLanguage(code: string): Observable<void> {
    return this.api.delete<void>(`/api/admin/languages/${code}`);
  }
}
