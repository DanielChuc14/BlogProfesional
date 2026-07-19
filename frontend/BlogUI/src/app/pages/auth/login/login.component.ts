import { Component, inject, signal } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { HttpErrorResponse } from '@angular/common/http';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';

declare const google: any;

@Component({
  selector: 'app-login',
  standalone: true,
  templateUrl: './login.component.html',
  imports: [RouterLink, FormsModule, TranslatePipe],
})
export class LoginComponent {
  private readonly auth   = inject(AuthService);
  private readonly toast  = inject(ToastService);
  private readonly router = inject(Router);

  email    = '';
  password = '';
  readonly loading        = signal(false);
  readonly googleLoading  = signal(false);
  readonly errorMsg       = signal('');

  submit(): void {
    if (!this.email || !this.password) return;
    this.loading.set(true);
    this.errorMsg.set('');

    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: () => {
        this.toast.success('Welcome back!');
        this.router.navigate(['/']);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.errorMsg.set(err.error?.error ?? err.error?.title ?? 'Invalid credentials.');
      },
    });
  }

  signInWithGoogle(): void {
    if (typeof google === 'undefined') {
      this.toast.error('Google Sign-In is not available. Check your connection.');
      return;
    }
    this.googleLoading.set(true);
    this.errorMsg.set('');
    google.accounts.id.initialize({
      client_id: '{{GOOGLE_CLIENT_ID}}',
      callback: (response: { credential: string }) => {
        this.auth.googleLogin(response.credential).subscribe({
          next: () => {
            this.googleLoading.set(false);
            this.toast.success('Welcome!');
            this.router.navigate(['/']);
          },
          error: (err: HttpErrorResponse) => {
            this.googleLoading.set(false);
            this.errorMsg.set(err.error?.error ?? err.error?.title ?? 'Google sign-in failed.');
          },
        });
      },
    });
    google.accounts.id.prompt();
  }
}
