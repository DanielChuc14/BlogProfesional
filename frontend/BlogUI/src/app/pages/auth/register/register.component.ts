import { Component, inject, signal } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { HttpErrorResponse } from '@angular/common/http';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { extractApiError } from '../../../core/utils/api-error';

@Component({
  selector: 'app-register',
  standalone: true,
  templateUrl: './register.component.html',
  imports: [RouterLink, FormsModule, TranslatePipe],
})
export class RegisterComponent {
  private readonly auth  = inject(AuthService);
  private readonly toast = inject(ToastService);

  displayName = '';
  username    = '';
  email       = '';
  password    = '';
  readonly loading  = signal(false);
  readonly success  = signal(false);
  readonly errorMsg = signal('');

  submit(): void {
    if (!this.displayName || !this.username || !this.email || !this.password) return;
    this.loading.set(true);
    this.errorMsg.set('');

    this.auth.register({ displayName: this.displayName, username: this.username, email: this.email, password: this.password }).subscribe({
      next: () => {
        this.loading.set(false);
        this.success.set(true);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.errorMsg.set(extractApiError(err, 'toast_registrationFailedPleaseTryAgain'));
      },
    });
  }
}
