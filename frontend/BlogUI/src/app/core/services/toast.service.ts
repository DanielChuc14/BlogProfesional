import { inject, Injectable, signal } from '@angular/core';
import { TranslationService } from './translation.service';

export type ToastType = 'success' | 'error' | 'info' | 'warning';

export interface Toast {
  id: number;
  type: ToastType;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly i18n = inject(TranslationService);

  private counter = 0;
  readonly toasts = signal<Toast[]>([]);

  success(message: string): void { this.add('success', message); }
  error(message: string): void   { this.add('error', message); }
  info(message: string): void    { this.add('info', message); }
  warning(message: string): void { this.add('warning', message); }

  remove(id: number): void {
    this.toasts.update(ts => ts.filter(t => t.id !== id));
  }

  // Los llamantes pasan una clave i18n ('toast_profileUpdated'). Los mensajes que
  // vienen del API no son claves, y translate() devuelve tal cual lo que no encuentra,
  // por lo que ambos casos funcionan sin que el llamante tenga que distinguirlos.
  private add(type: ToastType, message: string): void {
    const id = ++this.counter;
    this.toasts.update(ts => [...ts, { id, type, message: this.i18n.translate(message) }]);
    setTimeout(() => this.remove(id), 4000);
  }
}
