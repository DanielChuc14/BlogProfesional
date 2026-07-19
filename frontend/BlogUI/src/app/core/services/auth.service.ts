import { inject, Injectable, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError } from 'rxjs';
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
  // Reads and decrypts the stored session, then populates the signals.
  async init(): Promise<void> {
    const [token, user] = await Promise.all([
      this.storage.getItem<string>(TOKEN_KEY),
      this.storage.getItem<AuthResponse>(USER_KEY),
    ]);
    this._token.set(token);
    this._user.set(user);
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

  googleLogin(idToken: string): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('/api/auth/google', { idToken }).pipe(
      tap(res => this.saveSession(res)),
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
    const updated = { ...current, avatarUrl: url };
    this._user.set(updated);
    void this.storage.setItem(USER_KEY, updated);
  }

  clearSession(): void {
    this._token.set(null);
    this._user.set(null);
    this.storage.removeItem(TOKEN_KEY);
    this.storage.removeItem(USER_KEY);
  }

  private saveSession(res: AuthResponse): void {
    // Update signals immediately so the rest of the app reacts at once.
    // Persist to encrypted storage async in the background.
    this._token.set(res.accessToken);
    this._user.set(res);
    void this.storage.setItem(TOKEN_KEY, res.accessToken);
    void this.storage.setItem(USER_KEY, res);
  }
}
