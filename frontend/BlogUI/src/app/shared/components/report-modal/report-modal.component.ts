import { Component, inject, signal, output, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ReportService } from '../../../core/services/report.service';
import { ToastService } from '../../../core/services/toast.service';
import { ReportTargetType, ReportReason } from '../../../core/models';

@Component({
  selector: 'app-report-modal',
  standalone: true,
  templateUrl: './report-modal.component.html',
  imports: [FormsModule],
})
export class ReportModalComponent {
  readonly targetType = input.required<ReportTargetType>();
  readonly targetId   = input.required<string>();
  readonly closed     = output<void>();

  private readonly reportSvc = inject(ReportService);
  private readonly toast     = inject(ToastService);

  readonly submitting = signal(false);

  selectedReason: ReportReason = 'Spam';
  description = '';

  readonly reasons: { value: ReportReason; label: string }[] = [
    { value: 'Spam',          label: 'Spam' },
    { value: 'Harassment',    label: 'Harassment' },
    { value: 'HateSpeech',    label: 'Hate speech' },
    { value: 'FakeAccount',   label: 'Fake account' },
    { value: 'AdultContent',  label: 'Adult content' },
    { value: 'Copyright',     label: 'Copyright violation' },
    { value: 'Other',         label: 'Other' },
  ];

  submit(): void {
    if (this.submitting()) return;
    this.submitting.set(true);
    this.reportSvc.create({
      targetType:  this.targetType(),
      targetId:    this.targetId(),
      reason:      this.selectedReason,
      description: this.description.trim() || undefined,
    }).subscribe({
      next: () => {
        this.submitting.set(false);
        this.toast.success('Report submitted. Thank you.');
        this.closed.emit();
      },
      error: (err) => {
        this.submitting.set(false);
        const msg = err?.error?.error ?? 'Failed to submit report.';
        this.toast.error(msg);
      },
    });
  }

  cancel(): void {
    this.closed.emit();
  }
}
