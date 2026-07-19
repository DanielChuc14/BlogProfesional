import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { CursorPageResult, PostSummaryDto, TagDto } from '../models';

@Injectable({ providedIn: 'root' })
export class TagService {
  private readonly api = inject(ApiService);

  getTags(): Observable<TagDto[]> {
    return this.api.get<TagDto[]>('/api/tags');
  }

  createTag(name: string): Observable<TagDto> {
    return this.api.post<TagDto>('/api/tags', { name });
  }

  updateTag(id: string, name: string): Observable<TagDto> {
    return this.api.put<TagDto>(`/api/tags/${id}`, { name });
  }

  getPostsByTag(slug: string, cursor?: string, pageSize = 20): Observable<CursorPageResult<PostSummaryDto>> {
    return this.api.get<CursorPageResult<PostSummaryDto>>(`/api/tags/${slug}/posts`, { cursor, pageSize });
  }

  autocomplete(query: string): Observable<TagDto[]> {
    return this.api.get<TagDto[]>('/api/tags/autocomplete', { q: query });
  }

  search(query: string, tag?: string, cursor?: string): Observable<CursorPageResult<PostSummaryDto>> {
    return this.api.get<CursorPageResult<PostSummaryDto>>('/api/search', { q: query, tag, cursor });
  }
}
