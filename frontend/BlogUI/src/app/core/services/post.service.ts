import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  CursorPageResult,
  PostDetailDto,
  PostFeedQuery,
  PostSummaryDto,
  CreatePostRequest,
  UpdatePostRequest,
  SchedulePostRequest,
} from '../models';

@Injectable({ providedIn: 'root' })
export class PostService {
  private readonly api = inject(ApiService);

  getFeed(query: PostFeedQuery = {}): Observable<CursorPageResult<PostSummaryDto>> {
    return this.api.get<CursorPageResult<PostSummaryDto>>('/api/posts', query as Record<string, string>);
  }

  getBySlug(slug: string): Observable<PostDetailDto> {
    return this.api.get<PostDetailDto>(`/api/posts/${slug}`);
  }

  getById(id: string): Observable<PostDetailDto> {
    return this.api.get<PostDetailDto>(`/api/posts/${id}`);
  }

  create(req: CreatePostRequest): Observable<PostDetailDto> {
    return this.api.post<PostDetailDto>('/api/posts', req);
  }

  update(id: string, req: UpdatePostRequest): Observable<PostDetailDto> {
    return this.api.put<PostDetailDto>(`/api/posts/${id}`, req);
  }

  delete(id: string): Observable<void> {
    return this.api.delete<void>(`/api/posts/${id}`);
  }

  publish(id: string): Observable<void> {
    return this.api.patch<void>(`/api/posts/${id}/publish`);
  }

  schedule(id: string, req: SchedulePostRequest): Observable<void> {
    return this.api.patch<void>(`/api/posts/${id}/schedule`, req);
  }

  archive(id: string): Observable<void> {
    return this.api.patch<void>(`/api/posts/${id}/archive`);
  }

  like(id: string): Observable<void> {
    return this.api.post<void>(`/api/posts/${id}/like`);
  }

  unlike(id: string): Observable<void> {
    return this.api.delete<void>(`/api/posts/${id}/like`);
  }

  getPersonalizedFeed(cursor?: string, pageSize = 20): Observable<CursorPageResult<PostSummaryDto>> {
    return this.api.get<CursorPageResult<PostSummaryDto>>('/api/feed', { cursor, pageSize });
  }
}
