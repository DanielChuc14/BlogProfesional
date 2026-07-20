import { Component, inject, signal, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { HttpErrorResponse } from '@angular/common/http';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { extractApiError } from '../../../core/utils/api-error';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  templateUrl: './reset-password.component.html',
  imports: [FormsModule, TranslatePipe],
})
export class ResetPasswordComponent implements OnInit {
  private readonly auth   = inject(AuthService);
  private readonly toast  = inject(ToastService);
  private readonly router = inject(Router);
  private readonly route  = inject(ActivatedRoute);

  password = '';
  private token = '';
  private email = '';
  readonly loading  = signal(false);
  readonly errorMsg = signal('');

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
    this.email = this.route.snapshot.queryParamMap.get('email') ?? '';
  }

  submit(): void {
    if (!this.password) return;
    this.loading.set(true);
    this.errorMsg.set('');

    this.auth.resetPassword({ email: this.email, token: this.token, newPassword: this.password }).subscribe({
      next: () => {
        this.toast.success('toast_passwordUpdatedPleaseSignIn');
        this.router.navigate(['/login']);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.errorMsg.set(extractApiError(err, 'toast_resetFailedTheLinkMayHaveExpired'));
      },
    });
  }
}
