import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ReportDto, CreateReportRequest, PagedResult } from '../models';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly api = inject(ApiService);

  create(request: CreateReportRequest): Observable<ReportDto> {
    return this.api.post<ReportDto>('/api/reports', request);
  }

  getMine(page = 1, pageSize = 20): Observable<ReportDto[]> {
    return this.api.get<ReportDto[]>('/api/reports/mine', { page, pageSize });
  }

  // Admin
  getReports(status?: string, targetType?: string, page = 1, pageSize = 20): Observable<PagedResult<ReportDto>> {
    return this.api.get<PagedResult<ReportDto>>('/api/admin/reports', { status, targetType, page, pageSize });
  }

  getReport(id: string): Observable<ReportDto> {
    return this.api.get<ReportDto>(`/api/admin/reports/${id}`);
  }

  review(id: string, decision: 'Resolved' | 'Rejected', note?: string): Observable<void> {
    return this.api.put<void>(`/api/admin/reports/${id}/review`, { decision, note });
  }
}
