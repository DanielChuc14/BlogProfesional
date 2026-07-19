import { inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { ApiService } from './api.service';
import { CursorPageResult, NotificationDto } from '../models';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly api = inject(ApiService);

  readonly unreadCount = signal<number>(0);

  loadUnreadCount(): void {
    this.api.get<number>('/api/notifications/unread-count').subscribe({
      next: count => this.unreadCount.set(count),
      error: () => {},
    });
  }

  getNotifications(cursor?: string, pageSize = 20): Observable<CursorPageResult<NotificationDto>> {
    return this.api.get<CursorPageResult<NotificationDto>>('/api/notifications', { cursor, pageSize });
  }

  markRead(id: string): Observable<void> {
    return this.api.patch<void>(`/api/notifications/${id}/read`).pipe(
      tap(() => this.unreadCount.update(n => Math.max(0, n - 1))),
    );
  }

  markAllRead(): Observable<void> {
    return this.api.patch<void>('/api/notifications/read-all').pipe(
      tap(() => this.unreadCount.set(0)),
    );
  }
}
