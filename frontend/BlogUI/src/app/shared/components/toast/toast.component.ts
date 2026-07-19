import { Component, inject } from '@angular/core';
import { ToastService, Toast } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  templateUrl: './toast.component.html',
  imports: [],
})
export class ToastComponent {
  readonly toastSvc = inject(ToastService);

  toastClass(toast: Toast): string {
    const map: Record<string, string> = {
      success: 'bg-green-600 text-white',
      error:   'bg-red-600 text-white',
      warning: 'bg-yellow-500 text-white',
      info:    'bg-blue-600 text-white',
    };
    return map[toast.type] ?? 'bg-gray-700 text-white';
  }
}
