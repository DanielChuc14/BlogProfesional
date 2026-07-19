import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AnalyticsService } from '../../core/services/analytics.service';
import { NewsletterService, SendNewsletterResponse } from '../../core/services/newsletter.service';
import { ToastService } from '../../core/services/toast.service';
import { BloggerDashboardDto } from '../../core/models';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { TranslatePipe } from '../../core/pipes/translate.pipe';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  templateUrl: './dashboard.component.html',
  imports: [RouterLink, DatePipe, DecimalPipe, SpinnerComponent, FormsModule, TranslatePipe],
})
export class DashboardComponent implements OnInit {
  private readonly analyticsSvc   = inject(AnalyticsService);
  private readonly newsletterSvc  = inject(NewsletterService);
  private readonly toast          = inject(ToastService);

  readonly data    = signal<BloggerDashboardDto | null>(null);
  readonly loading = signal(true);

  // Newsletter
  newsletterSubject  = '';
  newsletterBody     = '';
  readonly nlSending  = signal(false);
  readonly nlPreview  = signal<SendNewsletterResponse | null>(null);

  private maxViews = 0;

  ngOnInit(): void {
    this.analyticsSvc.getBloggerDashboard().subscribe({
      next: d => {
        this.data.set(d);
        this.maxViews = Math.max(...d.dailyStats.map(s => s.viewCount), 1);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  barHeight(views: number): number {
    return Math.max(2, Math.round((views / this.maxViews) * 88));
  }

  initiateNewsletter(): void {
    if (!this.newsletterSubject.trim() || !this.newsletterBody.trim()) return;
    this.nlSending.set(true);
    this.newsletterSvc.initiate({ subject: this.newsletterSubject, htmlBody: this.newsletterBody }).subscribe({
      next: res => {
        this.nlPreview.set(res);
        this.nlSending.set(false);
      },
      error: (err) => {
        this.nlSending.set(false);
        this.toast.error(err.error?.error ?? err.error?.title ?? 'Failed to initiate newsletter.');
      },
    });
  }

  confirmNewsletter(): void {
    const preview = this.nlPreview();
    if (!preview) return;
    this.nlSending.set(true);
    this.newsletterSvc.confirm(preview.sendId).subscribe({
      next: () => {
        this.nlSending.set(false);
        this.nlPreview.set(null);
        this.newsletterSubject = '';
        this.newsletterBody    = '';
        this.toast.success('Newsletter sent successfully!');
      },
      error: (err) => {
        this.nlSending.set(false);
        this.toast.error(err.error?.error ?? err.error?.title ?? 'Failed to send newsletter.');
      },
    });
  }

  cancelNewsletter(): void {
    this.nlPreview.set(null);
  }
}
