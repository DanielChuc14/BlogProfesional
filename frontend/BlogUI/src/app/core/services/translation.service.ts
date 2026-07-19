import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { StorageService } from './storage.service';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class TranslationService {
  private readonly http    = inject(HttpClient);
  private readonly storage = inject(StorageService);
  private readonly api     = inject(ApiService);
  private readonly auth    = inject(AuthService);

  private translations: Record<string, string> = {};
  readonly currentLang = signal<string>('en');

  // Called after auth.init() so auth.user() is already populated.
  async init(): Promise<void> {
    const savedLang  = await this.storage.getItem<string>('lang');
    const serverLang = this.auth.user()?.preferredLanguage;
    const lang       = savedLang ?? serverLang ?? 'en';
    await this.loadLang(lang);
  }

  async setLanguage(lang: string): Promise<void> {
    await this.loadLang(lang);
    await this.storage.setItem('lang', lang);
    // Persist to backend (fire and forget — 401 if not logged in, ignored).
    this.api.put<void>('/api/users/me/language', { language: lang })
      .subscribe({ error: () => {} });
  }

  translate(key: string): string {
    return this.translations[key] ?? key;
  }

  private async loadLang(lang: string): Promise<void> {
    // 1. Try API (translations uploaded by admin via admin panel)
    try {
      this.translations = await firstValueFrom(
        this.api.get<Record<string, string>>(`/api/languages/${lang}/translations`)
      );
      this.currentLang.set(lang);
      return;
    } catch {}

    // 2. Fallback to static file (works for en/es while no DB upload exists)
    try {
      this.translations = await firstValueFrom(
        this.http.get<Record<string, string>>(`/i18n/${lang}.json`)
      );
      this.currentLang.set(lang);
      return;
    } catch {}

    // 3. If neither worked and not already 'en', fall back to English
    if (lang !== 'en') {
      try {
        this.translations = await firstValueFrom(
          this.api.get<Record<string, string>>('/api/languages/en/translations')
        );
      } catch {
        try {
          this.translations = await firstValueFrom(
            this.http.get<Record<string, string>>('/i18n/en.json')
          );
        } catch {}
      }
    }

    this.currentLang.set(lang);
  }
}
