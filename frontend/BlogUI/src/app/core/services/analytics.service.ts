import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { AdminDashboardDto, BloggerDashboardDto, PostAnalyticsDto } from '../models';

@Injectable({ providedIn: 'root' })
export class AnalyticsService {
  private readonly api = inject(ApiService);

  getBloggerDashboard(): Observable<BloggerDashboardDto> {
    return this.api.get<BloggerDashboardDto>('/api/analytics/me');
  }

  getPostAnalytics(postId: string): Observable<PostAnalyticsDto> {
    return this.api.get<PostAnalyticsDto>(`/api/analytics/posts/${postId}`);
  }

  getAdminDashboard(): Observable<AdminDashboardDto> {
    return this.api.get<AdminDashboardDto>('/api/admin/analytics');
  }
}
