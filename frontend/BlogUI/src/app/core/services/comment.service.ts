import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { CommentDto, CreateCommentRequest } from '../models';

@Injectable({ providedIn: 'root' })
export class CommentService {
  private readonly api = inject(ApiService);

  getByPost(postId: string): Observable<CommentDto[]> {
    return this.api.get<CommentDto[]>(`/api/posts/${postId}/comments`);
  }

  create(postId: string, req: CreateCommentRequest): Observable<CommentDto> {
    return this.api.post<CommentDto>(`/api/posts/${postId}/comments`, req);
  }

  delete(commentId: string): Observable<void> {
    return this.api.delete<void>(`/api/comments/${commentId}`);
  }

  like(commentId: string): Observable<void> {
    return this.api.post<void>(`/api/comments/${commentId}/like`);
  }

  unlike(commentId: string): Observable<void> {
    return this.api.delete<void>(`/api/comments/${commentId}/like`);
  }
}
