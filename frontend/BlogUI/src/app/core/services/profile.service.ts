import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  BlogProfileDto, UpdateProfileRequest, UpdateBlogThemeRequest, BlogThemeDto,
  BlogNoticeDto, CreateBlogNoticeRequest, QuickLinkDto, CreateQuickLinkRequest,
  UserWordFilterDto, AddWordFilterRequest, FollowerDto,
  UserPreferencesDto, UpdatePreferencesRequest,
} from '../models';

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private readonly api = inject(ApiService);

  getProfile(username: string): Observable<BlogProfileDto> {
    return this.api.get<BlogProfileDto>(`/api/users/${username}`);
  }

  updateProfile(req: UpdateProfileRequest): Observable<BlogProfileDto> {
    return this.api.put<BlogProfileDto>('/api/users/me', req);
  }

  uploadAvatar(file: File): Observable<BlogProfileDto> {
    const form = new FormData();
    form.append('file', file);
    return this.api.put<BlogProfileDto>('/api/users/me/avatar', form);
  }

  uploadBanner(file: File): Observable<BlogProfileDto> {
    const form = new FormData();
    form.append('file', file);
    return this.api.put<BlogProfileDto>('/api/users/me/banner', form);
  }

  follow(profileSlug: string): Observable<void> {
    return this.api.post<void>(`/api/users/${profileSlug}/follow`);
  }

  unfollow(profileSlug: string): Observable<void> {
    return this.api.delete<void>(`/api/users/${profileSlug}/follow`);
  }

  getFollowers(profileSlug: string, page = 1, pageSize = 20): Observable<FollowerDto[]> {
    return this.api.get<FollowerDto[]>(`/api/users/${profileSlug}/followers`, { page, pageSize });
  }

  getFollowing(profileSlug: string, page = 1, pageSize = 20): Observable<FollowerDto[]> {
    return this.api.get<FollowerDto[]>(`/api/users/${profileSlug}/following`, { page, pageSize });
  }

  // Theme
  getTheme(): Observable<BlogThemeDto> {
    return this.api.get<BlogThemeDto>('/api/users/me/theme');
  }

  updateTheme(req: UpdateBlogThemeRequest): Observable<BlogThemeDto> {
    return this.api.put<BlogThemeDto>('/api/users/me/theme', req);
  }

  // Word filters
  getWordFilters(): Observable<UserWordFilterDto[]> {
    return this.api.get<UserWordFilterDto[]>('/api/users/me/word-filters');
  }

  addWordFilter(req: AddWordFilterRequest): Observable<UserWordFilterDto> {
    return this.api.post<UserWordFilterDto>('/api/users/me/word-filters', req);
  }

  deleteWordFilter(id: string): Observable<void> {
    return this.api.delete<void>(`/api/users/me/word-filters/${id}`);
  }

  // Notices
  getNotices(): Observable<BlogNoticeDto[]> {
    return this.api.get<BlogNoticeDto[]>('/api/users/me/notices');
  }

  addNotice(req: CreateBlogNoticeRequest): Observable<BlogNoticeDto> {
    return this.api.post<BlogNoticeDto>('/api/users/me/notices', req);
  }

  updateNotice(id: string, req: CreateBlogNoticeRequest): Observable<BlogNoticeDto> {
    return this.api.put<BlogNoticeDto>(`/api/users/me/notices/${id}`, req);
  }

  deleteNotice(id: string): Observable<void> {
    return this.api.delete<void>(`/api/users/me/notices/${id}`);
  }

  // Quick links
  getQuickLinks(): Observable<QuickLinkDto[]> {
    return this.api.get<QuickLinkDto[]>('/api/users/me/quick-links');
  }

  addQuickLink(req: CreateQuickLinkRequest): Observable<QuickLinkDto> {
    return this.api.post<QuickLinkDto>('/api/users/me/quick-links', req);
  }

  deleteQuickLink(id: string): Observable<void> {
    return this.api.delete<void>(`/api/users/me/quick-links/${id}`);
  }

  // Preferences
  getPreferences(): Observable<UserPreferencesDto> {
    return this.api.get<UserPreferencesDto>('/api/users/me/preferences');
  }

  updatePreferences(req: UpdatePreferencesRequest): Observable<void> {
    return this.api.put<void>('/api/users/me/preferences', req);
  }

  updateLanguage(language: string): Observable<void> {
    return this.api.put<void>('/api/users/me/language', { language });
  }
}
