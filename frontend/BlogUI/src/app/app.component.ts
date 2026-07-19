import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './shared/components/header/header.component';
import { ToastComponent } from './shared/components/toast/toast.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, HeaderComponent, ToastComponent],
  template: `
    <app-header />
    <main class="min-h-screen">
      <router-outlet />
    </main>
    <app-toast />
  `,
})
export class AppComponent {}
