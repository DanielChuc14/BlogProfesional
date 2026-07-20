import { Component, inject, signal, OnInit, OnDestroy, input, ElementRef, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProfileService } from '../../core/services/profile.service';
import { PostService } from '../../core/services/post.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { BlockService } from '../../core/services/block.service';
import { ReportModalComponent } from '../../shared/components/report-modal/report-modal.component';
import {
  BlogProfileDto, BlogThemeDto, PostSummaryDto, SocialLinkDto,
  UpdateBlogThemeRequest, CreateBlogNoticeRequest, CreateQuickLinkRequest, AddWordFilterRequest,
  UserWordFilterDto, BlogNoticeDto
} from '../../core/models';
import { PostCardComponent } from '../../shared/components/post-card/post-card.component';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { TranslatePipe } from '../../core/pipes/translate.pipe';

@Component({
  selector: 'app-profile',
  standalone: true,
  templateUrl: './profile.component.html',
  imports: [RouterLink, PostCardComponent, SpinnerComponent, FormsModule, TranslatePipe, ReportModalComponent],
})
export class ProfileComponent implements OnInit, OnDestroy {
  readonly username = input.required<string>();

  private readonly profileSvc = inject(ProfileService);
  private readonly postSvc    = inject(PostService);
  private readonly toast      = inject(ToastService);
  private readonly blockSvc   = inject(BlockService);
  readonly auth               = inject(AuthService);
  private readonly elRef      = inject(ElementRef);
  private readonly platformId = inject(PLATFORM_ID);

  readonly profile      = signal<BlogProfileDto | null>(null);
  readonly posts        = signal<PostSummaryDto[]>([]);
  readonly loading      = signal(true);
  readonly postsLoading = signal(true);
  readonly saving          = signal(false);
  readonly followLoading   = signal(false);
  readonly blockLoading    = signal(false);
  readonly showReportModal = signal(false);
  readonly avatarSaving    = signal(false);
  readonly bannerSaving    = signal(false);

  // Tabs
  readonly activeTab = signal<'posts' | 'about' | 'lists' | 'settings'>('posts');

  // Profile edit
  readonly editMode  = signal(false);
  editDisplayName = '';
  editBio         = '';
  editAbout       = '';
  editTagline     = '';
  editWebsite     = '';
  editTwitter     = '';
  editLinkedIn    = '';
  editGitHub      = '';

  // Theme
  readonly themeMode = signal(false);
  editPrimaryColor   = '#0284c7';
  editSecondaryColor = '#0369a1';
  editAccentColor    = '#0ea5e9';
  editFontFamily     = 'sans';
  editLayoutStyle    = 'grid';
  editDarkMode       = false;
  readonly themeSaving = signal(false);

  // Notices
  readonly noticesMode = signal(false);
  readonly notices = signal<BlogNoticeDto[]>([]);
  readonly noticesLoading = signal(false);
  newNoticeTitle   = '';
  newNoticeContent = '';
  newNoticeType    = 'Info';
  newNoticeActive  = true;
  readonly noticeSaving = signal(false);

  // Quick links
  readonly quickLinksMode = signal(false);
  newLinkTitle = '';
  newLinkUrl   = '';
  newLinkIcon  = '';
  readonly linkSaving = signal(false);

  // Word filters
  readonly filtersMode = signal(false);
  readonly wordFilters = signal<UserWordFilterDto[]>([]);
  readonly filtersLoading = signal(false);
  newFilterWord = '';
  readonly filterSaving = signal(false);

  ngOnInit(): void {
    this.profileSvc.getProfile(this.username()).subscribe({
      next: p => {
        this.profile.set(p);
        this.loading.set(false);
        this.applyTheme(p.theme);
        this.loadPosts(p.username);
      },
      error: () => this.loading.set(false),
    });
  }

  ngOnDestroy(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    const el = this.elRef.nativeElement as HTMLElement;
    el.style.removeProperty('--blog-primary');
    el.style.removeProperty('--blog-secondary');
    el.style.removeProperty('--blog-accent');
  }

  private applyTheme(theme: BlogThemeDto | null | undefined): void {
    if (!isPlatformBrowser(this.platformId)) return;
    const el = this.elRef.nativeElement as HTMLElement;
    el.style.setProperty('--blog-primary',   theme?.primaryColor   ?? '#0284c7');
    el.style.setProperty('--blog-secondary', theme?.secondaryColor ?? '#0369a1');
    el.style.setProperty('--blog-accent',    theme?.accentColor    ?? '#0ea5e9');
  }

  get postsGridClass(): string {
    switch (this.profile()?.theme?.layoutStyle) {
      case 'list':     return 'flex flex-col gap-4';
      case 'magazine': return 'grid gap-6 sm:grid-cols-3';
      default:         return 'grid gap-6 sm:grid-cols-2';
    }
  }

  get fontClass(): string {
    switch (this.profile()?.theme?.fontFamily) {
      case 'serif': return 'font-serif';
      case 'mono':  return 'font-mono';
      default:      return 'font-sans';
    }
  }

  private loadPosts(username: string): void {
    this.postSvc.getFeed({ author: username, pageSize: 20 }).subscribe({
      next: res => { this.posts.set(res.items); this.postsLoading.set(false); },
      error: () => this.postsLoading.set(false),
    });
  }

  setTab(tab: 'posts' | 'about' | 'lists' | 'settings'): void {
    this.activeTab.set(tab);
    if (tab === 'settings' && this.isOwner()) {
      if (this.wordFilters().length === 0 && !this.filtersLoading()) this.loadWordFilters();
    }
  }

  isOwner(): boolean {
    return this.auth.user()?.username === this.profile()?.username;
  }

  // ── Avatar / Banner upload ────────────────────────────────────────────

  onAvatarSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    this.avatarSaving.set(true);
    this.profileSvc.uploadAvatar(file).subscribe({
      next: p => {
        this.profile.set(p);
        if (p.avatarUrl) this.auth.updateAvatarUrl(p.avatarUrl);
        this.avatarSaving.set(false);
        this.toast.success('toast_photoUpdated');
      },
      error: () => {
        this.avatarSaving.set(false);
        this.toast.error('toast_failedToUpdatePhoto');
      },
    });
  }

  onBannerSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    this.bannerSaving.set(true);
    this.profileSvc.uploadBanner(file).subscribe({
      next: p => {
        this.profile.set(p);
        this.bannerSaving.set(false);
        this.toast.success('toast_bannerUpdated');
      },
      error: () => {
        this.bannerSaving.set(false);
        this.toast.error('toast_failedToUpdateBanner');
      },
    });
  }

  // ── Profile Edit ───────────────────────────────────────────────────────

  openEdit(): void {
    const p = this.profile();
    if (!p) return;
    this.editDisplayName = p.displayName;
    this.editBio         = p.bio ?? '';
    this.editAbout       = p.about ?? '';
    this.editTagline     = p.tagline ?? '';
    this.editWebsite     = p.socialLinks.find(s => s.platform === 'Website')?.url ?? '';
    this.editTwitter     = p.socialLinks.find(s => s.platform === 'Twitter')?.url ?? '';
    this.editLinkedIn    = p.socialLinks.find(s => s.platform === 'LinkedIn')?.url ?? '';
    this.editGitHub      = p.socialLinks.find(s => s.platform === 'GitHub')?.url ?? '';
    this.editMode.set(true);
  }

  cancelEdit(): void { this.editMode.set(false); }

  saveEdit(): void {
    if (!this.editDisplayName.trim()) return;
    const socialLinks: SocialLinkDto[] = [];
    if (this.editWebsite.trim())  socialLinks.push({ platform: 'Website',  url: this.editWebsite.trim() });
    if (this.editTwitter.trim())  socialLinks.push({ platform: 'Twitter',  url: this.editTwitter.trim() });
    if (this.editLinkedIn.trim()) socialLinks.push({ platform: 'LinkedIn', url: this.editLinkedIn.trim() });
    if (this.editGitHub.trim())   socialLinks.push({ platform: 'GitHub',   url: this.editGitHub.trim() });

    this.saving.set(true);
    this.profileSvc.updateProfile({
      displayName: this.editDisplayName.trim(),
      bio:     this.editBio || undefined,
      about:   this.editAbout || undefined,
      tagline: this.editTagline || undefined,
      socialLinks,
    }).subscribe({
      next: updated => {
        this.profile.set(updated);
        this.saving.set(false);
        this.editMode.set(false);
        this.toast.success('toast_profileUpdated');
      },
      error: () => {
        this.saving.set(false);
        this.toast.error('toast_failedToUpdateProfile');
      },
    });
  }

  // ── Theme ──────────────────────────────────────────────────────────────

  openTheme(): void {
    const t = this.profile()?.theme;
    this.editPrimaryColor   = t?.primaryColor ?? '#0284c7';
    this.editSecondaryColor = t?.secondaryColor ?? '#0369a1';
    this.editAccentColor    = t?.accentColor ?? '#0ea5e9';
    this.editFontFamily     = t?.fontFamily ?? 'sans';
    this.editLayoutStyle    = t?.layoutStyle ?? 'grid';
    this.editDarkMode       = t?.darkModeDefault ?? false;
    this.themeMode.set(true);
  }

  cancelTheme(): void { this.themeMode.set(false); }

  saveTheme(): void {
    this.themeSaving.set(true);
    const req: UpdateBlogThemeRequest = {
      primaryColor:   this.editPrimaryColor,
      secondaryColor: this.editSecondaryColor,
      accentColor:    this.editAccentColor,
      fontFamily:     this.editFontFamily,
      layoutStyle:    this.editLayoutStyle,
      darkModeDefault: this.editDarkMode,
    };
    this.profileSvc.updateTheme(req).subscribe({
      next: theme => {
        this.profile.update(p => p ? { ...p, theme } : p);
        this.applyTheme(theme);
        this.themeSaving.set(false);
        this.themeMode.set(false);
        this.toast.success('toast_themeSaved');
      },
      error: () => {
        this.themeSaving.set(false);
        this.toast.error('toast_failedToSaveTheme');
      },
    });
  }

  // ── Notices ────────────────────────────────────────────────────────────

  openNotices(): void {
    this.noticesMode.set(true);
    if (this.notices().length === 0) this.loadNotices();
  }

  closeNotices(): void { this.noticesMode.set(false); }

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
      title: this.newNoticeTitle.trim(),
      content: this.newNoticeContent.trim(),
      type: this.newNoticeType,
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
        this.profile.update(p => p ? {
          ...p,
          activeNotices: p.activeNotices.filter(n => n.id !== id)
        } : p);
        this.toast.success('toast_noticeDeleted');
      },
      error: () => this.toast.error('toast_failedToDeleteNotice'),
    });
  }

  // ── Quick Links ────────────────────────────────────────────────────────

  openQuickLinks(): void { this.quickLinksMode.set(true); }
  closeQuickLinks(): void { this.quickLinksMode.set(false); }

  addQuickLink(): void {
    if (!this.newLinkTitle.trim() || !this.newLinkUrl.trim()) return;
    this.linkSaving.set(true);
    const req: CreateQuickLinkRequest = {
      title: this.newLinkTitle.trim(),
      url:   this.newLinkUrl.trim(),
      icon:  this.newLinkIcon || null,
      order: (this.profile()?.quickLinks.length ?? 0),
    };
    this.profileSvc.addQuickLink(req).subscribe({
      next: link => {
        this.profile.update(p => p ? { ...p, quickLinks: [...p.quickLinks, link] } : p);
        this.newLinkTitle = '';
        this.newLinkUrl = '';
        this.newLinkIcon = '';
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
        this.profile.update(p => p ? { ...p, quickLinks: p.quickLinks.filter(l => l.id !== id) } : p);
        this.toast.success('toast_linkDeleted');
      },
      error: () => this.toast.error('toast_failedToDeleteLink'),
    });
  }

  // ── Word Filters ───────────────────────────────────────────────────────

  openFilters(): void {
    this.filtersMode.set(true);
    this.loadWordFilters();
  }

  closeFilters(): void { this.filtersMode.set(false); }

  private loadWordFilters(): void {
    this.filtersLoading.set(true);
    this.profileSvc.getWordFilters().subscribe({
      next: list => { this.wordFilters.set(list); this.filtersLoading.set(false); },
      error: () => this.filtersLoading.set(false),
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
        this.toast.success('toast_wordFilterAdded');
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

  // ── Follow ─────────────────────────────────────────────────────────────

  toggleFollow(): void {
    const p = this.profile();
    if (!p || this.followLoading()) return;
    this.followLoading.set(true);
    if (p.isFollowing) {
      this.profileSvc.unfollow(p.slug).subscribe({
        next: () => {
          this.profile.update(pr => pr ? { ...pr, isFollowing: false, followersCount: pr.followersCount - 1 } : pr);
          this.followLoading.set(false);
        },
        error: () => { this.toast.error('toast_failedToUnfollow'); this.followLoading.set(false); },
      });
    } else {
      this.profileSvc.follow(p.slug).subscribe({
        next: () => {
          this.profile.update(pr => pr ? { ...pr, isFollowing: true, followersCount: pr.followersCount + 1 } : pr);
          this.followLoading.set(false);
        },
        error: () => { this.toast.error('toast_failedToFollow'); this.followLoading.set(false); },
      });
    }
  }

  // ── Report ─────────────────────────────────────────────────────────────

  openReportModal(): void  { this.showReportModal.set(true); }
  closeReportModal(): void { this.showReportModal.set(false); }

  // ── Block ──────────────────────────────────────────────────────────────

  toggleBlock(): void {
    const p = this.profile();
    if (!p || this.blockLoading()) return;
    this.blockLoading.set(true);
    if (p.isBlocked) {
      this.blockSvc.unblock(p.userId).subscribe({
        next: () => {
          this.profile.update(pr => pr ? { ...pr, isBlocked: false } : pr);
          this.blockLoading.set(false);
          this.toast.success('toast_userUnblocked');
        },
        error: () => { this.toast.error('toast_failedToUnblock'); this.blockLoading.set(false); },
      });
    } else {
      this.blockSvc.block(p.userId).subscribe({
        next: () => {
          this.profile.update(pr => pr ? { ...pr, isBlocked: true, isFollowing: false } : pr);
          this.blockLoading.set(false);
          this.toast.success('toast_userBlocked');
        },
        error: () => { this.toast.error('toast_failedToBlock'); this.blockLoading.set(false); },
      });
    }
  }

  noticeTypeClass(type: string): string {
    switch (type.toLowerCase()) {
      case 'warning': return 'bg-yellow-50 border-yellow-400 text-yellow-800';
      case 'success': return 'bg-green-50 border-green-400 text-green-800';
      case 'promo':   return 'bg-purple-50 border-purple-400 text-purple-800';
      default:        return 'bg-blue-50 border-blue-400 text-blue-800';
    }
  }
}
