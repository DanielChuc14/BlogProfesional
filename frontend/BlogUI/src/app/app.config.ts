import { ApplicationConfig, provideAppInitializer, provideZonelessChangeDetection, inject } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { AuthService } from './core/services/auth.service';
import { TranslationService } from './core/services/translation.service';
import { LanguageService } from './core/services/language.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withFetch(), withInterceptors([authInterceptor])),
    // Order: auth → languages → translations
    // auth.init() populates user session first.
    // language.loadLanguages() fetches the active language list from API.
    // t.init() uses auth.user().preferredLanguage to pick which language to load.
    provideAppInitializer(async () => {
      const auth = inject(AuthService);
      const langSvc = inject(LanguageService);
      const t = inject(TranslationService);
      await auth.init();
      await langSvc.loadLanguages();
      await t.init();
    }),
  ],
};
