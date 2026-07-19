import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface SendNewsletterRequest {
  subject: string;
  htmlBody: string;
}

export interface SendNewsletterResponse {
  sendId: string;
  estimatedRecipients: number;
  canSendAfter: string;
}

export interface NewsletterSendDto {
  id: string;
  subject: string;
  status: string;
  estimatedRecipients: number;
  actualRecipients: number;
  sentAt: string | null;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class NewsletterService {
  private readonly api = inject(ApiService);

  initiate(req: SendNewsletterRequest): Observable<SendNewsletterResponse> {
    return this.api.post<SendNewsletterResponse>('/api/newsletter/send', req);
  }

  confirm(sendId: string): Observable<NewsletterSendDto> {
    return this.api.post<NewsletterSendDto>(`/api/newsletter/confirm/${sendId}`);
  }
}
