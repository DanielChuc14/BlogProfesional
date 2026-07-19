import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { HttpErrorResponse } from '@angular/common/http';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  templateUrl: './forgot-password.component.html',
  imports: [RouterLink, FormsModule, TranslatePipe],
})
export class ForgotPasswordComponent {
  private readonly auth = inject(AuthService);

  email = '';
  readonly loading  = signal(false);
  readonly sent     = signal(false);
  readonly errorMsg = signal('');

  submit(): void {
    if (!this.email) return;
    this.loading.set(true);
    this.errorMsg.set('');

    this.auth.forgotPassword({ email: this.email }).subscribe({
      next: () => { this.loading.set(false); this.sent.set(true); },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.errorMsg.set(err.error?.error ?? err.error?.title ?? 'Something went wrong.');
      },
    });
  }
}
