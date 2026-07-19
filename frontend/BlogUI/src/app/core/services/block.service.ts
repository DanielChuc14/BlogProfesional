import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { BlockedUserDto } from '../models';

@Injectable({ providedIn: 'root' })
export class BlockService {
  private readonly api = inject(ApiService);

  block(targetUserId: string): Observable<void> {
    return this.api.post<void>(`/api/blocks/${targetUserId}`);
  }

  unblock(targetUserId: string): Observable<void> {
    return this.api.delete<void>(`/api/blocks/${targetUserId}`);
  }

  getBlocked(page = 1, pageSize = 20): Observable<BlockedUserDto[]> {
    return this.api.get<BlockedUserDto[]>('/api/blocks', { page, pageSize });
  }
}
