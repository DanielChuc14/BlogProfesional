import { Component, inject, signal, OnInit, HostListener, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { RouterLink, RouterLinkActive, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { StorageService } from '../../../core/services/storage.service';
import { TranslationService } from '../../../core/services/translation.service';
import { LanguageService } from '../../../core/services/language.service';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { NotificationDto } from '../../../core/models';

@Component({
  selector: 'app-header',
  standalone: true,
  templateUrl: './header.component.html',
  imports: [RouterLink, RouterLinkActive, FormsModule, TranslatePipe],
})
export class HeaderComponent implements OnInit {
  readonly auth               = inject(AuthService);
  readonly notifSvc           = inject(NotificationService);
  readonly t                  = inject(TranslationService);
  readonly langSvc            = inject(LanguageService);
  private readonly router     = inject(Router);
  private readonly storage    = inject(StorageService);
  private readonly platformId = inject(PLATFORM_ID);

  searchQuery = '';

  readonly notifOpen         = signal(false);
  readonly notifications     = signal<NotificationDto[]>([]);
  readonly notifLoading      = signal(false);
  readonly mobileMenuOpen    = signal(false);
  readonly darkMode          = signal(false);
  readonly userMenuOpen      = signal(false);

  async ngOnInit(): Promise<void> {
    if (this.auth.isLoggedIn()) {
      this.notifSvc.loadUnreadCount();
    }
    if (isPlatformBrowser(this.platformId)) {
      const saved = await this.storage.getItem<string>('darkMode');
      if (saved === 'true') {
        this.darkMode.set(true);
        document.documentElement.classList.add('dark');
      }
    }
  }

  toggleNotifications(): void {
    if (!this.notifOpen()) {
      this.loadNotifications();
    }
    this.notifOpen.update(v => !v);
  }

  private loadNotifications(): void {
    this.notifLoading.set(true);
    this.notifSvc.getNotifications(undefined, 10).subscribe({
      next: res => {
        this.notifications.set(res.items);
        this.notifLoading.set(false);
      },
      error: () => this.notifLoading.set(false),
    });
  }

  markAllRead(): void {
    this.notifSvc.markAllRead().subscribe({
      next: () => this.notifications.update(list => list.map(n => ({ ...n, isRead: true }))),
    });
  }

  notifLabel(type: string): string {
    return this.t.translate(`notif_${type.toLowerCase()}`);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(e: Event): void {
    const target = e.target as HTMLElement;
    if (!target.closest('.notif-panel-wrapper')) {
      this.notifOpen.set(false);
    }
    if (!target.closest('.user-menu-wrapper')) {
      this.userMenuOpen.set(false);
    }
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update(v => !v);
  }

  toggleDarkMode(): void {
    const next = !this.darkMode();
    this.darkMode.set(next);
    if (isPlatformBrowser(this.platformId)) {
      if (next) document.documentElement.classList.add('dark');
      else document.documentElement.classList.remove('dark');
      void this.storage.setItem('darkMode', String(next));
    }
  }

  toggleUserMenu(): void {
    this.userMenuOpen.update(v => !v);
  }

  setLang(lang: string): void {
    void this.t.setLanguage(lang);
  }

  submitSearch(): void {
    const q = this.searchQuery.trim();
    if (!q) return;
    this.router.navigate(['/search'], { queryParams: { q } });
    this.searchQuery = '';
  }

  doLogout(): void {
    this.userMenuOpen.set(false);
    this.auth.logout().subscribe();
  }
}
