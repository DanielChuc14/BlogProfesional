import { inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ApiService } from './api.service';

export interface LanguageDto {
  code: string;
  name: string;
  nativeName: string;
}

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly api = inject(ApiService);

  readonly activeLanguages = signal<LanguageDto[]>([]);
  private loaded = false;

  async loadLanguages(): Promise<void> {
    if (this.loaded) return;
    try {
      const langs = await firstValueFrom(
        this.api.get<LanguageDto[]>('/api/languages')
      );
      this.activeLanguages.set(langs);
      this.loaded = true;
    } catch {
      // Fallback si la API no está disponible al arrancar
      this.activeLanguages.set([
        { code: 'en', name: 'English', nativeName: 'English' },
        { code: 'es', name: 'Spanish', nativeName: 'Español' },
      ]);
    }
  }
}
