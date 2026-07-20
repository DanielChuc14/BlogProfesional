import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProfileService } from '../../core/services/profile.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { TranslatePipe } from '../../core/pipes/translate.pipe';
import { ImageCropperModalComponent } from '../../shared/components/image-cropper-modal/image-cropper-modal.component';
import {
  BlogProfileDto, BlogNoticeDto, QuickLinkDto, UserWordFilterDto,
  UpdateBlogThemeRequest, CreateBlogNoticeRequest, CreateQuickLinkRequest,
  AddWordFilterRequest, SocialLinkDto
} from '../../core/models';

type SettingsTab = 'profile' | 'appearance' | 'notices' | 'links' | 'filters';

@Component({
  selector: 'app-blog-settings',
  standalone: true,
  templateUrl: './blog-settings.component.html',
  imports: [RouterLink, FormsModule, SpinnerComponent, TranslatePipe, ImageCropperModalComponent],
})
export class BlogSettingsComponent implements OnInit {
  private readonly profileSvc = inject(ProfileService);
  private readonly toast      = inject(ToastService);
  readonly auth               = inject(AuthService);

  readonly loading  = signal(true);
  readonly profile  = signal<BlogProfileDto | null>(null);
  readonly activeTab = signal<SettingsTab>('profile');

  // ── Image crop modal ───────────────────────────────────────────────────
  readonly cropFile       = signal<File | null>(null);
  readonly cropAspect     = signal<number>(1);
  readonly cropResize     = signal<number>(512);
  readonly cropTitle      = signal<string>('Adjust image');
  private pendingCropType: 'avatar' | 'banner' | null = null;

  // ── Profile ────────────────────────────────────────────────────────────
  profileDisplayName = '';
  profileBio         = '';
  profileAbout       = '';
  profileTagline     = '';
  profileLogoUrl     = '';
  profileBannerUrl   = '';
  profileWebsite     = '';
  profileTwitter     = '';
  profileLinkedIn    = '';
  profileGitHub      = '';
  readonly profileSaving = signal(false);

  // ── Appearance ─────────────────────────────────────────────────────────
  themePrimary   = '#0284c7';
  themeSecondary = '#0369a1';
  themeAccent    = '#0ea5e9';
  themeFontFamily  = 'sans';
  themeLayoutStyle = 'grid';
  themeDarkMode    = false;
  readonly themeSaving = signal(false);

  // ── Notices ────────────────────────────────────────────────────────────
  readonly notices        = signal<BlogNoticeDto[]>([]);
  readonly noticesLoading = signal(false);
  newNoticeTitle   = '';
  newNoticeContent = '';
  newNoticeType    = 'Info';
  newNoticeActive  = true;
  readonly noticeSaving = signal(false);

  // ── Quick Links ────────────────────────────────────────────────────────
  readonly quickLinks        = signal<QuickLinkDto[]>([]);
  readonly quickLinksLoading = signal(false);
  newLinkTitle = '';
  newLinkUrl   = '';
  newLinkIcon  = '';
  readonly linkSaving = signal(false);

  // ── Word Filters ───────────────────────────────────────────────────────
  readonly wordFilters        = signal<UserWordFilterDto[]>([]);
  readonly wordFiltersLoading = signal(false);
  newFilterWord = '';
  readonly filterSaving = signal(false);

  ngOnInit(): void {
    const username = this.auth.user()?.username;
    if (!username) { this.loading.set(false); return; }

    this.profileSvc.getProfile(username).subscribe({
      next: p => {
        this.profile.set(p);
        this.loading.set(false);
        this.seedProfileForm(p);
        this.seedThemeForm(p);
        this.quickLinks.set(p.quickLinks ?? []);
        this.notices.set(p.activeNotices ?? []);
        this.loadNotices();
        this.loadWordFilters();
      },
      error: () => this.loading.set(false),
    });
  }

  setTab(tab: SettingsTab): void {
    this.activeTab.set(tab);
  }

  // ── Profile ────────────────────────────────────────────────────────────

  private seedProfileForm(p: BlogProfileDto): void {
    this.profileDisplayName = p.displayName;
    this.profileBio         = p.bio ?? '';
    this.profileAbout       = p.about ?? '';
    this.profileTagline     = p.tagline ?? '';
    this.profileLogoUrl     = p.logoUrl ?? '';
    this.profileBannerUrl   = p.bannerUrl ?? '';
    this.profileWebsite     = p.socialLinks.find(s => s.platform === 'Website')?.url ?? '';
    this.profileTwitter     = p.socialLinks.find(s => s.platform === 'Twitter')?.url ?? '';
    this.profileLinkedIn    = p.socialLinks.find(s => s.platform === 'LinkedIn')?.url ?? '';
    this.profileGitHub      = p.socialLinks.find(s => s.platform === 'GitHub')?.url  ?? '';
  }

  onAvatarChange(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    this.pendingCropType = 'avatar';
    this.cropTitle.set('Adjust profile photo');
    this.cropAspect.set(1);
    this.cropResize.set(512);
    this.cropFile.set(file);
    (event.target as HTMLInputElement).value = '';
  }

  onBannerChange(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    this.pendingCropType = 'banner';
    this.cropTitle.set('Adjust cover photo');
    this.cropAspect.set(3);
    this.cropResize.set(1200);
    this.cropFile.set(file);
    (event.target as HTMLInputElement).value = '';
  }

  onCropConfirmed(blob: Blob): void {
    const type = this.pendingCropType;
    this.cropFile.set(null);
    this.pendingCropType = null;

    const file = new File([blob], 'image.jpg', { type: 'image/jpeg' });

    if (type === 'avatar') {
      this.profileSvc.uploadAvatar(file).subscribe({
        next: p => {
          this.profile.set(p);
          if (p.avatarUrl) this.auth.updateAvatarUrl(p.avatarUrl);
          this.toast.success('toast_profilePhotoUpdated');
        },
        error: () => this.toast.error('toast_failedToUploadPhoto'),
      });
    } else if (type === 'banner') {
      this.profileSvc.uploadBanner(file).subscribe({
        next: p => {
          this.profile.set(p);
          this.toast.success('toast_coverPhotoUpdated');
        },
        error: () => this.toast.error('toast_failedToUploadCover'),
      });
    }
  }

  onCropCancelled(): void {
    this.cropFile.set(null);
    this.pendingCropType = null;
  }

  saveProfile(): void {
    if (!this.profileDisplayName.trim()) return;
    const socialLinks: SocialLinkDto[] = [];
    if (this.profileWebsite.trim())  socialLinks.push({ platform: 'Website',  url: this.profileWebsite.trim() });
    if (this.profileTwitter.trim())  socialLinks.push({ platform: 'Twitter',  url: this.profileTwitter.trim() });
    if (this.profileLinkedIn.trim()) socialLinks.push({ platform: 'LinkedIn', url: this.profileLinkedIn.trim() });
    if (this.profileGitHub.trim())   socialLinks.push({ platform: 'GitHub',   url: this.profileGitHub.trim() });

    this.profileSaving.set(true);
    this.profileSvc.updateProfile({
      displayName: this.profileDisplayName.trim(),
      bio:       this.profileBio   || undefined,
      about:     this.profileAbout || undefined,
      tagline:   this.profileTagline   || undefined,
      logoUrl:   this.profileLogoUrl   || undefined,
      bannerUrl: this.profileBannerUrl || undefined,
      socialLinks,
    }).subscribe({
      next: updated => {
        this.profile.set(updated);
        this.profileSaving.set(false);
        this.toast.success('toast_profileSaved');
      },
      error: () => {
        this.profileSaving.set(false);
        this.toast.error('toast_failedToSaveProfile');
      },
    });
  }

  // ── Appearance ─────────────────────────────────────────────────────────

  private seedThemeForm(p: BlogProfileDto): void {
    const t = p.theme;
    this.themePrimary    = t?.primaryColor   ?? '#0284c7';
    this.themeSecondary  = t?.secondaryColor ?? '#0369a1';
    this.themeAccent     = t?.accentColor    ?? '#0ea5e9';
    this.themeFontFamily = t?.fontFamily     ?? 'sans';
    this.themeLayoutStyle = t?.layoutStyle   ?? 'grid';
    this.themeDarkMode   = t?.darkModeDefault ?? false;
  }

  saveTheme(): void {
    this.themeSaving.set(true);
    const req: UpdateBlogThemeRequest = {
      primaryColor:    this.themePrimary,
      secondaryColor:  this.themeSecondary,
      accentColor:     this.themeAccent,
      fontFamily:      this.themeFontFamily,
      layoutStyle:     this.themeLayoutStyle,
      darkModeDefault: this.themeDarkMode,
    };
    this.profileSvc.updateTheme(req).subscribe({
      next: theme => {
        this.profile.update(p => p ? { ...p, theme } : p);
        this.themeSaving.set(false);
        this.toast.success('toast_appearanceSaved');
      },
      error: () => {
        this.themeSaving.set(false);
        this.toast.error('toast_failedToSaveAppearance');
      },
    });
  }

  // ── Notices ────────────────────────────────────────────────────────────

  private loadNotices(): void {
    this.noticesLoading.set(true);
    this.profileSvc.getNotices().subscribe({
      next: list => { this.notices.set(list); this.noticesLoading.set(false); },
      error: () => this.noticesLoading.set(false),
    });
  }

  addNotice(): void {
    if (!this.newNoticeTitle.trim() || !this.newNoticeContent.trim()) return;
    this.noticeSaving.set(true);
    const req: CreateBlogNoticeRequest = {
      title:    this.newNoticeTitle.trim(),
      content:  this.newNoticeContent.trim(),
      type:     this.newNoticeType,
      isActive: this.newNoticeActive,
      priority: 0,
    };
    this.profileSvc.addNotice(req).subscribe({
      next: notice => {
        this.notices.update(list => [notice, ...list]);
        this.newNoticeTitle = '';
        this.newNoticeContent = '';
        this.noticeSaving.set(false);
        this.toast.success('toast_noticeAdded');
      },
      error: () => {
        this.noticeSaving.set(false);
        this.toast.error('toast_failedToAddNotice');
      },
    });
  }

  deleteNotice(id: string): void {
    this.profileSvc.deleteNotice(id).subscribe({
      next: () => {
        this.notices.update(list => list.filter(n => n.id !== id));
        this.toast.success('toast_noticeDeleted');
      },
      error: () => this.toast.error('toast_failedToDeleteNotice'),
    });
  }

  noticeTypeClass(type: string): string {
    switch (type.toLowerCase()) {
      case 'warning': return 'bg-yellow-50 border-yellow-400 text-yellow-800';
      case 'success': return 'bg-green-50  border-green-400  text-green-800';
      case 'promo':   return 'bg-purple-50 border-purple-400 text-purple-800';
      default:        return 'bg-blue-50   border-blue-400   text-blue-800';
    }
  }

  // ── Quick Links ────────────────────────────────────────────────────────

  addQuickLink(): void {
    if (!this.newLinkTitle.trim() || !this.newLinkUrl.trim()) return;
    this.linkSaving.set(true);
    const req: CreateQuickLinkRequest = {
      title: this.newLinkTitle.trim(),
      url:   this.newLinkUrl.trim(),
      icon:  this.newLinkIcon || null,
      order: this.quickLinks().length,
    };
    this.profileSvc.addQuickLink(req).subscribe({
      next: link => {
        this.quickLinks.update(list => [...list, link]);
        this.newLinkTitle = '';
        this.newLinkUrl   = '';
        this.newLinkIcon  = '';
        this.linkSaving.set(false);
        this.toast.success('toast_linkAdded');
      },
      error: () => {
        this.linkSaving.set(false);
        this.toast.error('toast_failedToAddLink');
      },
    });
  }

  deleteQuickLink(id: string): void {
    this.profileSvc.deleteQuickLink(id).subscribe({
      next: () => {
        this.quickLinks.update(list => list.filter(l => l.id !== id));
        this.toast.success('toast_linkDeleted');
      },
      error: () => this.toast.error('toast_failedToDeleteLink'),
    });
  }

  // ── Word Filters ───────────────────────────────────────────────────────

  private loadWordFilters(): void {
    this.wordFiltersLoading.set(true);
    this.profileSvc.getWordFilters().subscribe({
      next: list => { this.wordFilters.set(list); this.wordFiltersLoading.set(false); },
      error: () => this.wordFiltersLoading.set(false),
    });
  }

  addWordFilter(): void {
    if (!this.newFilterWord.trim()) return;
    this.filterSaving.set(true);
    this.profileSvc.addWordFilter({ word: this.newFilterWord.trim() }).subscribe({
      next: filter => {
        this.wordFilters.update(list => [...list, filter]);
        this.newFilterWord = '';
        this.filterSaving.set(false);
        this.toast.success('toast_filterAdded');
      },
      error: () => {
        this.filterSaving.set(false);
        this.toast.error('toast_failedToAddFilter');
      },
    });
  }

  deleteWordFilter(id: string): void {
    this.profileSvc.deleteWordFilter(id).subscribe({
      next: () => {
        this.wordFilters.update(list => list.filter(f => f.id !== id));
        this.toast.success('toast_filterRemoved');
      },
      error: () => this.toast.error('toast_failedToRemoveFilter'),
    });
  }
}
