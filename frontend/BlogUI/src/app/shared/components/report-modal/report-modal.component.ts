import { Component, inject, signal, output, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ReportService } from '../../../core/services/report.service';
import { ToastService } from '../../../core/services/toast.service';
import { ReportTargetType, ReportReason } from '../../../core/models';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';

@Component({
  selector: 'app-report-modal',
  standalone: true,
  templateUrl: './report-modal.component.html',
  imports: [FormsModule, TranslatePipe],
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

  // label es una clave i18n; el template la pasa por el pipe translate.
  readonly reasons: { value: ReportReason; label: string }[] = [
    { value: 'Spam',          label: 'report_reasonSpam' },
    { value: 'Harassment',    label: 'report_reasonHarassment' },
    { value: 'HateSpeech',    label: 'report_reasonHateSpeech' },
    { value: 'FakeAccount',   label: 'report_reasonFakeAccount' },
    { value: 'AdultContent',  label: 'report_reasonAdultContent' },
    { value: 'Copyright',     label: 'report_reasonCopyright' },
    { value: 'Other',         label: 'report_reasonOther' },
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
        this.toast.success('toast_reportSubmittedThankYou');
        this.closed.emit();
      },
      error: (err) => {
        this.submitting.set(false);
        this.toast.error(err?.error?.error ?? 'report_submitFailed');
      },
    });
  }

  cancel(): void {
    this.closed.emit();
  }
}
