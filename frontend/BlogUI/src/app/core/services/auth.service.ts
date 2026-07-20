import { inject, Injectable, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError, firstValueFrom } from 'rxjs';
import { ApiService } from './api.service';
import { StorageService } from './storage.service';
import {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  ForgotPasswordRequest,
  ResetPasswordRequest,
} from '../models';

const TOKEN_KEY = 'bp_access_token';
const USER_KEY  = 'bp_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api     = inject(ApiService);
  private readonly router  = inject(Router);
  private readonly storage = inject(StorageService);

  private readonly _user  = signal<AuthResponse | null>(null);
  private readonly _token = signal<string | null>(null);

  readonly user         = this._user.asReadonly();
  readonly token        = this._token.asReadonly();
  readonly isLoggedIn   = computed(() => !!this._token());
  readonly roles        = computed(() => this._user()?.roles ?? []);
  readonly isBlogger    = computed(() => this.roles().some(r => ['Blogger', 'Admin', 'SuperAdmin'].includes(r)));
  readonly isAdmin      = computed(() => this.roles().some(r => ['Admin', 'SuperAdmin'].includes(r)));
  readonly isSuperAdmin = computed(() => this.roles().includes('SuperAdmin'));

  // Called by APP_INITIALIZER before any component renders.
  // Attempts a silent refresh: the HttpOnly cookie rebuilds the session without
  // ever persisting the access token in storage. If there is no valid cookie,
  // refresh() clears the session and the app starts anonymous.
  async init(): Promise<void> {
    try {
      await firstValueFrom(this.refresh());
    } catch {
      this.clearSession();
    }
  }

  register(req: RegisterRequest): Observable<void> {
    return this.api.post<void>('/api/auth/register', req);
  }

  login(req: LoginRequest): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('/api/auth/login', req).pipe(
      tap(res => this.saveSession(res)),
    );
  }

  refresh(): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('/api/auth/refresh', {}).pipe(
      tap(res => this.saveSession(res)),
      catchError(err => {
        this.clearSession();
        return throwError(() => err);
      }),
    );
  }

  logout(): Observable<void> {
    return this.api.post<void>('/api/auth/logout', {}).pipe(
      tap(() => {
        this.clearSession();
        this.router.navigate(['/login']);
      }),
    );
  }

  forgotPassword(req: ForgotPasswordRequest): Observable<void> {
    return this.api.post<void>('/api/auth/forgot-password', req);
  }

  resetPassword(req: ResetPasswordRequest): Observable<void> {
    return this.api.post<void>('/api/auth/reset-password', req);
  }

  confirmEmail(token: string, email: string): Observable<void> {
    return this.api.get<void>('/api/auth/confirm-email', { token, email });
  }

  updateAvatarUrl(url: string): void {
    const current = this._user();
    if (!current) return;
    this._user.set({ ...current, avatarUrl: url });
  }

  clearSession(): void {
    this._token.set(null);
    this._user.set(null);
    this.storage.removeItem(TOKEN_KEY);
    this.storage.removeItem(USER_KEY);
  }

  private saveSession(res: AuthResponse): void {
    // Access token lives only in memory (the signal); it is never persisted, so
    // an XSS cannot steal it from storage. On reload the session is rebuilt from
    // the HttpOnly refresh cookie in init().
    this._token.set(res.accessToken);
    this._user.set(res);
  }
}
