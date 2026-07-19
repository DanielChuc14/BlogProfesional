import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { ProfileService } from '../../core/services/profile.service';
import { TranslationService } from '../../core/services/translation.service';
import { LanguageService } from '../../core/services/language.service';
import { ToastService } from '../../core/services/toast.service';
import { BlockedUsersComponent } from './blocked-users/blocked-users.component';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { TranslatePipe } from '../../core/pipes/translate.pipe';
import { UserPreferencesDto } from '../../core/models';

@Component({
  selector: 'app-settings',
  standalone: true,
  templateUrl: './settings.component.html',
  imports: [FormsModule, BlockedUsersComponent, SpinnerComponent, TranslatePipe],
})
export class SettingsComponent implements OnInit {
  readonly auth        = inject(AuthService);
  readonly profileSvc  = inject(ProfileService);
  readonly t           = inject(TranslationService);
  readonly langSvc     = inject(LanguageService);
  private readonly toast = inject(ToastService);

  readonly tabs = [
    { id: 'account', label: 'settings_tab_account' },
    { id: 'privacy',  label: 'settings_tab_privacy' },
  ];

  readonly activeTab = signal('account');

  // Account
  readonly prefsLoading = signal(true);
  readonly prefsSaving  = signal(false);
  readonly avatarSaving = signal(false);
  readonly bannerSaving = signal(false);
  readonly bannerUrl    = signal<string | null>(null);

  displayName = '';
  bio         = '';
  selectedLang = '';

  // Privacy
  readonly privacySaving          = signal(false);
  receiveEmailNotifications       = true;
  profileVisibility               = 'public';

  private prefsLoaded = false;

  ngOnInit(): void {
    const user = this.auth.user();
    if (user) {
      this.displayName  = user.displayName;
      this.selectedLang = user.preferredLanguage;
    }
    this.loadPreferences();
  }

  setTab(id: string): void {
    this.activeTab.set(id);
  }

  // ── Account ───────────────────────────────────────────────────────────

  private loadPreferences(): void {
    this.prefsLoading.set(true);

    // Load profile to get bannerUrl and bio
    const username = this.auth.user()?.username;
    if (username) {
      this.profileSvc.getProfile(username).subscribe({
        next: p => {
          this.bannerUrl.set(p.bannerUrl);
          this.bio = p.bio ?? '';
        },
      });
    }

    this.profileSvc.getPreferences().subscribe({
      next: prefs => {
        this.receiveEmailNotifications = prefs.receiveEmailNotifications;
        this.profileVisibility         = prefs.profileVisibility;
        this.selectedLang              = prefs.preferredLanguage;
        this.prefsLoaded = true;
        this.prefsLoading.set(false);
      },
      error: () => this.prefsLoading.set(false),
    });
  }

  saveAccount(): void {
    this.prefsSaving.set(true);
    this.profileSvc.updateProfile({ displayName: this.displayName, bio: this.bio || undefined }).subscribe({
      next: () => {
        // If language changed, persist and apply
        const currentLang = this.t.currentLang();
        if (this.selectedLang !== currentLang) {
          void this.t.setLanguage(this.selectedLang);
        }
        this.prefsSaving.set(false);
        this.toast.success(this.t.translate('settings_saved'));
      },
      error: () => {
        this.prefsSaving.set(false);
        this.toast.error(this.t.translate('settings_saveFailed'));
      },
    });
  }

  onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file  = input.files?.[0];
    if (!file) return;
    this.avatarSaving.set(true);
    this.profileSvc.uploadAvatar(file).subscribe({
      next: profile => {
        if (profile.avatarUrl) this.auth.updateAvatarUrl(profile.avatarUrl);
        this.avatarSaving.set(false);
        this.toast.success(this.t.translate('settings_saved'));
      },
      error: () => {
        this.avatarSaving.set(false);
        this.toast.error(this.t.translate('settings_saveFailed'));
      },
    });
  }

  onBannerSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file  = input.files?.[0];
    if (!file) return;
    this.bannerSaving.set(true);
    this.profileSvc.uploadBanner(file).subscribe({
      next: profile => {
        this.bannerUrl.set(profile.bannerUrl);
        this.bannerSaving.set(false);
        this.toast.success(this.t.translate('settings_saved'));
      },
      error: () => {
        this.bannerSaving.set(false);
        this.toast.error(this.t.translate('settings_saveFailed'));
      },
    });
  }

  // ── Privacy ───────────────────────────────────────────────────────────

  savePrivacy(): void {
    this.privacySaving.set(true);
    this.profileSvc.updatePreferences({
      receiveEmailNotifications: this.receiveEmailNotifications,
      profileVisibility: this.profileVisibility,
    }).subscribe({
      next: () => {
        this.privacySaving.set(false);
        this.toast.success(this.t.translate('settings_saved'));
      },
      error: () => {
        this.privacySaving.set(false);
        this.toast.error(this.t.translate('settings_saveFailed'));
      },
    });
  }
}
