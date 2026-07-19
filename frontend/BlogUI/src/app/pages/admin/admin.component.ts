import { Component, inject, signal, OnInit } from '@angular/core';
import { DecimalPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AnalyticsService } from '../../core/services/analytics.service';
import { AdminService, UserSummary, RoleDto, AdminPostSummary, CommentSummary, RestrictedWordDto, UserSuspensionDto, AuditLogDto, AdminLanguageDto, CreateLanguageRequest, UpdateLanguageRequest } from '../../core/services/admin.service';
import { ReportService } from '../../core/services/report.service';
import { ToastService } from '../../core/services/toast.service';
import { AdminDashboardDto, TagDto, ReportDto, PagedResult } from '../../core/models';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { TranslatePipe } from '../../core/pipes/translate.pipe';

@Component({
  selector: 'app-admin',
  standalone: true,
  templateUrl: './admin.component.html',
  imports: [SpinnerComponent, DecimalPipe, DatePipe, FormsModule, TranslatePipe],
})
export class AdminComponent implements OnInit {
  private readonly analyticsSvc = inject(AnalyticsService);
  private readonly adminSvc     = inject(AdminService);
  private readonly reportSvc    = inject(ReportService);
  private readonly toast        = inject(ToastService);

  readonly tabs = [
    { id: 'dashboard',        label: 'admin_tab_dashboard' },
    { id: 'users',            label: 'admin_tab_users' },
    { id: 'roles',            label: 'admin_tab_roles' },
    { id: 'tags',             label: 'admin_tab_tags' },
    { id: 'posts',            label: 'admin_tab_posts' },
    { id: 'comments',         label: 'admin_tab_comments' },
    { id: 'settings',         label: 'admin_tab_settings' },
    { id: 'restricted-words', label: 'admin_tab_restrictedWords' },
    { id: 'reports',          label: 'admin_tab_reports' },
    { id: 'timeline',         label: 'admin_tab_timeline' },
    { id: 'languages',        label: 'admin_tab_languages' },
  ];

  readonly activeTab = signal('dashboard');

  // Dashboard
  readonly stats        = signal<AdminDashboardDto | null>(null);
  readonly statsLoading = signal(true);

  // Users
  readonly users           = signal<UserSummary[]>([]);
  readonly usersLoading    = signal(false);
  readonly usersPage       = signal(1);
  readonly usersTotalPages = signal(1);
  readonly selectedUser    = signal<UserSummary | null>(null);
  userSearch   = '';
  roleToAssign = '';

  // Roles
  readonly roles        = signal<RoleDto[]>([]);
  readonly rolesLoading = signal(false);
  newRoleName = '';

  // Tags
  readonly tags        = signal<TagDto[]>([]);
  readonly tagsLoading = signal(false);
  newTagName     = '';
  readonly editingTagId   = signal<string | null>(null);
  editingTagName = '';

  // Posts
  readonly adminPosts      = signal<AdminPostSummary[]>([]);
  readonly postsLoading    = signal(false);
  readonly postsPage       = signal(1);
  readonly postsTotalPages = signal(1);
  postsStatusFilter = '';

  // Comments
  readonly comments           = signal<CommentSummary[]>([]);
  readonly commentsLoading    = signal(false);
  readonly commentsPage       = signal(1);
  readonly commentsTotalPages = signal(1);

  // Settings
  readonly settings        = signal<{ key: string; value: string; updatedAt: string }[]>([]);
  readonly settingsLoading = signal(false);
  readonly settingsSaving  = signal(false);
  settingsEdits: Record<string, string> = {};

  // Reports
  readonly reports           = signal<ReportDto[]>([]);
  readonly reportsLoading    = signal(false);
  readonly reportsPage       = signal(1);
  readonly reportsTotalPages = signal(1);
  reportsStatusFilter   = '';
  reportsTypeFilter     = '';
  readonly reviewingReport  = signal<ReportDto | null>(null);
  reviewDecision = '';
  reviewNote     = '';
  readonly reviewSaving = signal(false);

  // Restricted words
  readonly restrictedWords        = signal<RestrictedWordDto[]>([]);
  readonly restrictedWordsLoading = signal(false);
  newRestrictedPhrase    = '';
  newRestrictedIsRegex   = false;
  newRestrictedSeverity: 'Warn' | 'Block' = 'Block';
  readonly restrictedSaving = signal(false);

  // Timeline (audit logs)
  readonly auditLogs          = signal<AuditLogDto[]>([]);
  readonly auditLogsLoading   = signal(false);
  readonly auditLogsPage      = signal(1);
  readonly auditLogsTotalPages = signal(1);

  // Suspensions
  readonly suspendingUser       = signal<UserSummary | null>(null);
  readonly suspensionHistory    = signal<UserSuspensionDto[]>([]);
  suspendReason   = '';
  suspendDays     = 7;
  readonly suspendSaving = signal(false);

  // Languages
  readonly languages        = signal<AdminLanguageDto[]>([]);
  readonly languagesLoading = signal(false);
  readonly languageSaving   = signal(false);
  readonly editingLanguage  = signal<AdminLanguageDto | null>(null);
  readonly creatingLanguage = signal(false);

  newLangCode       = '';
  newLangName       = '';
  newLangNativeName = '';
  newLangIsActive   = true;

  editLangName       = '';
  editLangNativeName = '';

  selectedTranslationFile: File | null = null;
  uploadingTranslationFor: string | null = null;

  // Confirm dialog
  readonly confirmMsg = signal('');
  private confirmCallback?: () => void;

  ngOnInit(): void {
    this.loadStats();
  }

  setTab(id: string): void {
    this.activeTab.set(id);
    if (id === 'users'    && this.users().length === 0)       this.loadUsers(1);
    if (id === 'roles'    && this.roles().length === 0)       this.loadRoles();
    if (id === 'tags'     && this.tags().length === 0)        this.loadTags();
    if (id === 'posts'    && this.adminPosts().length === 0)  this.loadAdminPosts(1);
    if (id === 'comments' && this.comments().length === 0)    this.loadComments(1);
    if (id === 'settings' && this.settings().length === 0)    this.loadSettings();
    if (id === 'restricted-words' && this.restrictedWords().length === 0) this.loadRestrictedWords();
    if (id === 'reports'          && this.reports().length === 0)          this.loadReports(1);
    if (id === 'timeline'         && this.auditLogs().length === 0)        this.loadAuditLogs(1);
    if (id === 'languages'        && this.languages().length === 0)        this.loadLanguages();
  }

  // ── Dashboard ─────────────────────────────────────────────────────────

  private loadStats(): void {
    this.analyticsSvc.getAdminDashboard().subscribe({
      next: d => { this.stats.set(d); this.statsLoading.set(false); },
      error: () => this.statsLoading.set(false),
    });
  }

  // ── Users ─────────────────────────────────────────────────────────────

  loadUsers(page: number): void {
    this.usersLoading.set(true);
    this.usersPage.set(page);
    this.adminSvc.getUsers(page, 20, this.userSearch || undefined).subscribe({
      next: res => {
        this.users.set(res.items);
        this.usersTotalPages.set(res.totalPages);
        this.usersLoading.set(false);
      },
      error: () => this.usersLoading.set(false),
    });
  }

  searchUsers(): void { this.loadUsers(1); }

  usersGoToPage(page: number): void { this.loadUsers(page); }

  openUserDetail(user: UserSummary): void {
    this.selectedUser.set({ ...user });
    this.roleToAssign = '';
    if (this.roles().length === 0) this.loadRoles();
  }

  assignRoleToUser(): void {
    const user = this.selectedUser();
    if (!user || !this.roleToAssign) return;
    const role = this.roleToAssign;
    this.adminSvc.assignRole(user.id, role).subscribe({
      next: () => {
        this.selectedUser.update(u => u ? { ...u, roles: [...u.roles, role] } : u);
        this.users.update(list => list.map(u => u.id === user.id ? { ...u, roles: [...u.roles, role] } : u));
        this.roleToAssign = '';
        this.toast.success('Role assigned.');
      },
      error: () => this.toast.error('Failed to assign role.'),
    });
  }

  removeRoleFromUser(role: string): void {
    const user = this.selectedUser();
    if (!user) return;
    this.adminSvc.removeRole(user.id, role).subscribe({
      next: () => {
        this.selectedUser.update(u => u ? { ...u, roles: u.roles.filter(r => r !== role) } : u);
        this.users.update(list => list.map(u => u.id === user.id ? { ...u, roles: u.roles.filter(r => r !== role) } : u));
        this.toast.success('Role removed.');
      },
      error: () => this.toast.error('Failed to remove role.'),
    });
  }

  changeUserRole(user: UserSummary, newRole: string): void {
    this.adminSvc.changeRole(user.id, newRole).subscribe({
      next: () => {
        this.users.update(list => list.map(u =>
          u.id === user.id
            ? { ...u, roles: [...u.roles.filter(r => !['Reader','Blogger','Admin'].includes(r)), newRole] }
            : u
        ));
        if (this.selectedUser()?.id === user.id) {
          this.selectedUser.update(u => u
            ? { ...u, roles: [...u.roles.filter(r => !['Reader','Blogger','Admin'].includes(r)), newRole] }
            : u);
        }
        this.toast.success(`Role changed to ${newRole}.`);
      },
      error: () => this.toast.error('Failed to change role.'),
    });
  }

  banUser(user: UserSummary): void {
    this.adminSvc.banUser(user.id).subscribe({
      next: () => {
        this.users.update(list => list.map(u => u.id === user.id ? { ...u, isActive: false } : u));
        this.toast.success(`${user.username} has been banned.`);
      },
      error: () => this.toast.error('Failed to ban user.'),
    });
  }

  unbanUser(user: UserSummary): void {
    this.adminSvc.unbanUser(user.id).subscribe({
      next: () => {
        this.users.update(list => list.map(u => u.id === user.id ? { ...u, isActive: true } : u));
        this.toast.success(`${user.username} has been unbanned.`);
      },
      error: () => this.toast.error('Failed to unban user.'),
    });
  }

  confirmDeleteUser(user: UserSummary): void {
    this.confirmMsg.set(`Delete user "${user.username}"? This cannot be undone.`);
    this.confirmCallback = () => {
      this.adminSvc.deleteUser(user.id).subscribe({
        next: () => {
          this.users.update(list => list.filter(u => u.id !== user.id));
          this.toast.success('User deleted.');
        },
        error: () => this.toast.error('Failed to delete user.'),
      });
    };
  }

  // ── Roles ─────────────────────────────────────────────────────────────

  private loadRoles(): void {
    this.rolesLoading.set(true);
    this.adminSvc.getRoles().subscribe({
      next: r => { this.roles.set(r); this.rolesLoading.set(false); },
      error: () => this.rolesLoading.set(false),
    });
  }

  createRole(): void {
    const name = this.newRoleName.trim();
    if (!name) return;
    this.adminSvc.createRole(name).subscribe({
      next: role => {
        this.roles.update(r => [...r, role]);
        this.newRoleName = '';
        this.toast.success(`Role "${role.name}" created.`);
      },
      error: () => this.toast.error('Failed to create role.'),
    });
  }

  deleteRole(name: string): void {
    this.confirmMsg.set(`Delete role "${name}"?`);
    this.confirmCallback = () => {
      this.adminSvc.deleteRole(name).subscribe({
        next: () => {
          this.roles.update(r => r.filter(x => x.name !== name));
          this.toast.success('Role deleted.');
        },
        error: () => this.toast.error('Failed to delete role.'),
      });
    };
  }

  // ── Tags ──────────────────────────────────────────────────────────────

  private loadTags(): void {
    this.tagsLoading.set(true);
    this.adminSvc.getAdminTags().subscribe({
      next: t => { this.tags.set(t); this.tagsLoading.set(false); },
      error: () => this.tagsLoading.set(false),
    });
  }

  createTag(): void {
    const name = this.newTagName.trim();
    if (!name) return;
    this.adminSvc.createAdminTag(name).subscribe({
      next: tag => {
        this.tags.update(t => [...t, tag]);
        this.newTagName = '';
        this.toast.success(`Tag "${tag.name}" created.`);
      },
      error: () => this.toast.error('Failed to create tag.'),
    });
  }

  startEditTag(tag: TagDto): void {
    this.editingTagId.set(tag.id);
    this.editingTagName = tag.name;
  }

  cancelEditTag(): void {
    this.editingTagId.set(null);
    this.editingTagName = '';
  }

  saveTag(id: string): void {
    const name = this.editingTagName.trim();
    if (!name) return;
    this.adminSvc.updateAdminTag(id, name).subscribe({
      next: updated => {
        this.tags.update(list => list.map(t => t.id === id ? updated : t));
        this.editingTagId.set(null);
        this.toast.success('Tag updated.');
      },
      error: () => this.toast.error('Failed to update tag.'),
    });
  }

  deleteTag(id: string): void {
    this.confirmMsg.set('Delete this tag? Posts using it will lose the tag.');
    this.confirmCallback = () => {
      this.adminSvc.deleteTag(id).subscribe({
        next: () => {
          this.tags.update(t => t.filter(x => x.id !== id));
          this.toast.success('Tag deleted.');
        },
        error: () => this.toast.error('Failed to delete tag.'),
      });
    };
  }

  // ── Posts ─────────────────────────────────────────────────────────────

  loadAdminPosts(page: number): void {
    this.postsLoading.set(true);
    this.postsPage.set(page);
    this.adminSvc.getAdminPosts(page, 20, this.postsStatusFilter || undefined).subscribe({
      next: res => {
        this.adminPosts.set(res.items);
        this.postsTotalPages.set(res.totalPages);
        this.postsLoading.set(false);
      },
      error: () => this.postsLoading.set(false),
    });
  }

  unpublishPost(post: AdminPostSummary): void {
    this.confirmMsg.set(`Unpublish "${post.title}"? It will be archived.`);
    this.confirmCallback = () => {
      this.adminSvc.unpublishPost(post.id, 'Unpublished by admin').subscribe({
        next: () => {
          this.adminPosts.update(list => list.map(p => p.id === post.id ? { ...p, status: 'Archived' } : p));
          this.toast.success('Post unpublished.');
        },
        error: () => this.toast.error('Failed to unpublish post.'),
      });
    };
  }

  forceDeletePost(id: string): void {
    this.confirmMsg.set('Force delete this post? This cannot be undone.');
    this.confirmCallback = () => {
      this.adminSvc.forceDeletePost(id).subscribe({
        next: () => {
          this.adminPosts.update(p => p.filter(x => x.id !== id));
          this.toast.success('Post deleted.');
        },
        error: () => this.toast.error('Failed to delete post.'),
      });
    };
  }

  // ── Comments ──────────────────────────────────────────────────────────

  loadComments(page: number): void {
    this.commentsLoading.set(true);
    this.commentsPage.set(page);
    this.adminSvc.getAdminComments(page, 20).subscribe({
      next: res => {
        this.comments.set(res.items);
        this.commentsTotalPages.set(res.totalPages);
        this.commentsLoading.set(false);
      },
      error: () => this.commentsLoading.set(false),
    });
  }

  deleteComment(id: string): void {
    this.confirmMsg.set('Delete this comment? This cannot be undone.');
    this.confirmCallback = () => {
      this.adminSvc.deleteComment(id).subscribe({
        next: () => {
          this.comments.update(list => list.filter(c => c.id !== id));
          this.toast.success('Comment deleted.');
        },
        error: () => this.toast.error('Failed to delete comment.'),
      });
    };
  }

  // ── Settings ──────────────────────────────────────────────────────────

  private loadSettings(): void {
    this.settingsLoading.set(true);
    this.adminSvc.getSettings().subscribe({
      next: list => {
        this.settings.set(list);
        this.settingsEdits = Object.fromEntries(list.map(s => [s.key, s.value]));
        this.settingsLoading.set(false);
      },
      error: () => this.settingsLoading.set(false),
    });
  }

  saveSettings(): void {
    this.settingsSaving.set(true);
    this.adminSvc.updateSettings(this.settingsEdits).subscribe({
      next: () => {
        this.settingsSaving.set(false);
        this.toast.success('Settings saved.');
        this.settings.update(list => list.map(s => ({ ...s, value: this.settingsEdits[s.key] ?? s.value })));
      },
      error: () => {
        this.settingsSaving.set(false);
        this.toast.error('Failed to save settings.');
      },
    });
  }

  addSetting(): void {
    const key = prompt('Setting key (e.g. platform:name):');
    if (!key?.trim()) return;
    if (this.settings().some(s => s.key === key.trim())) {
      this.toast.error('Setting key already exists.');
      return;
    }
    const newSetting = { key: key.trim(), value: '', updatedAt: new Date().toISOString() };
    this.settings.update(list => [...list, newSetting]);
    this.settingsEdits[key.trim()] = '';
  }

  // ── Restricted Words ──────────────────────────────────────────────────

  private loadRestrictedWords(): void {
    this.restrictedWordsLoading.set(true);
    this.adminSvc.getRestrictedWords().subscribe({
      next: list => { this.restrictedWords.set(list); this.restrictedWordsLoading.set(false); },
      error: () => this.restrictedWordsLoading.set(false),
    });
  }

  addRestrictedWord(): void {
    const phrase = this.newRestrictedPhrase.trim();
    if (!phrase) return;
    this.restrictedSaving.set(true);
    this.adminSvc.addRestrictedWord(phrase, this.newRestrictedIsRegex, this.newRestrictedSeverity).subscribe({
      next: word => {
        this.restrictedWords.update(list => [...list, word]);
        this.newRestrictedPhrase    = '';
        this.newRestrictedIsRegex   = false;
        this.newRestrictedSeverity  = 'Block';
        this.restrictedSaving.set(false);
        this.toast.success('Restricted word added.');
      },
      error: () => {
        this.restrictedSaving.set(false);
        this.toast.error('Failed to add restricted word.');
      },
    });
  }

  deleteRestrictedWord(id: string): void {
    this.confirmMsg.set('Remove this restricted word?');
    this.confirmCallback = () => {
      this.adminSvc.deleteRestrictedWord(id).subscribe({
        next: () => {
          this.restrictedWords.update(list => list.filter(w => w.id !== id));
          this.toast.success('Restricted word removed.');
        },
        error: () => this.toast.error('Failed to remove restricted word.'),
      });
    };
  }

  // ── Reports ───────────────────────────────────────────────────────────

  loadReports(page: number): void {
    this.reportsLoading.set(true);
    this.reportsPage.set(page);
    this.reportSvc.getReports(
      this.reportsStatusFilter || undefined,
      this.reportsTypeFilter   || undefined,
      page, 20
    ).subscribe({
      next: res => {
        this.reports.set(res.items);
        this.reportsTotalPages.set(res.totalPages);
        this.reportsLoading.set(false);
      },
      error: () => this.reportsLoading.set(false),
    });
  }

  openReview(report: ReportDto): void {
    this.reviewingReport.set(report);
    this.reviewDecision = '';
    this.reviewNote     = '';
  }

  closeReview(): void {
    this.reviewingReport.set(null);
  }

  submitReview(): void {
    const report = this.reviewingReport();
    if (!report || !this.reviewDecision) return;
    this.reviewSaving.set(true);
    this.reportSvc.review(report.id, this.reviewDecision as 'Resolved' | 'Rejected', this.reviewNote || undefined).subscribe({
      next: () => {
        this.reports.update(list => list.map(r =>
          r.id === report.id ? { ...r, status: this.reviewDecision, adminNote: this.reviewNote } : r
        ));
        this.reviewSaving.set(false);
        this.closeReview();
        this.toast.success('Report reviewed.');
      },
      error: () => {
        this.reviewSaving.set(false);
        this.toast.error('Failed to review report.');
      },
    });
  }

  isSuspended(user: UserSummary): boolean {
    return !!user.suspendedUntil && new Date(user.suspendedUntil) > new Date();
  }

  // ── Timeline ──────────────────────────────────────────────────────────

  loadAuditLogs(page: number): void {
    this.auditLogsLoading.set(true);
    this.auditLogsPage.set(page);
    this.adminSvc.getAuditLogs(page, 30).subscribe({
      next: res => {
        this.auditLogs.set(res.items);
        this.auditLogsTotalPages.set(res.totalPages);
        this.auditLogsLoading.set(false);
      },
      error: () => this.auditLogsLoading.set(false),
    });
  }

  // ── Suspensions ───────────────────────────────────────────────────────

  openSuspendModal(user: UserSummary): void {
    this.suspendingUser.set(user);
    this.suspendReason = '';
    this.suspendDays   = 7;
    this.adminSvc.getSuspensionHistory(user.id).subscribe({
      next: list => this.suspensionHistory.set(list),
      error: () => this.suspensionHistory.set([]),
    });
  }

  closeSuspendModal(): void {
    this.suspendingUser.set(null);
    this.suspensionHistory.set([]);
  }

  submitSuspension(): void {
    const user = this.suspendingUser();
    if (!user || !this.suspendReason.trim() || this.suspendDays < 1) return;
    this.suspendSaving.set(true);
    this.adminSvc.suspendUser(user.id, { reason: this.suspendReason.trim(), durationDays: this.suspendDays }).subscribe({
      next: () => {
        const expiresAt = new Date(Date.now() + this.suspendDays * 86400000).toISOString();
        this.users.update(list => list.map(u => u.id === user.id ? { ...u, suspendedUntil: expiresAt } : u));
        this.suspendSaving.set(false);
        this.closeSuspendModal();
        this.toast.success(`${user.username} suspended for ${this.suspendDays} day(s).`);
      },
      error: () => {
        this.suspendSaving.set(false);
        this.toast.error('Failed to suspend user.');
      },
    });
  }

  liftSuspension(user: UserSummary): void {
    this.adminSvc.liftSuspension(user.id).subscribe({
      next: () => {
        this.users.update(list => list.map(u => u.id === user.id ? { ...u, suspendedUntil: undefined } : u));
        if (this.suspendingUser()?.id === user.id) {
          this.suspendingUser.update(u => u ? { ...u, suspendedUntil: undefined } : u);
          this.adminSvc.getSuspensionHistory(user.id).subscribe({ next: list => this.suspensionHistory.set(list) });
        }
        this.toast.success(`Suspension lifted for ${user.username}.`);
      },
      error: () => this.toast.error('Failed to lift suspension.'),
    });
  }

  // ── Languages ─────────────────────────────────────────────────────────

  loadLanguages(): void {
    this.languagesLoading.set(true);
    this.adminSvc.getAdminLanguages().subscribe({
      next: list => { this.languages.set(list); this.languagesLoading.set(false); },
      error: () => this.languagesLoading.set(false),
    });
  }

  openCreateLanguage(): void {
    this.creatingLanguage.set(true);
    this.newLangCode = '';
    this.newLangName = '';
    this.newLangNativeName = '';
    this.newLangIsActive = true;
  }

  closeCreateLanguage(): void {
    this.creatingLanguage.set(false);
  }

  submitCreateLanguage(): void {
    const code = this.newLangCode.trim();
    const name = this.newLangName.trim();
    const nativeName = this.newLangNativeName.trim();
    if (!code || !name || !nativeName) return;

    this.languageSaving.set(true);
    const req: CreateLanguageRequest = { code, name, nativeName, isActive: this.newLangIsActive };
    this.adminSvc.createLanguage(req).subscribe({
      next: lang => {
        this.languages.update(list => [...list, lang]);
        this.languageSaving.set(false);
        this.closeCreateLanguage();
        this.toast.success(`Language '${lang.code}' created.`);
      },
      error: () => {
        this.languageSaving.set(false);
        this.toast.error('Failed to create language.');
      },
    });
  }

  openEditLanguage(lang: AdminLanguageDto): void {
    this.editingLanguage.set(lang);
    this.editLangName = lang.name;
    this.editLangNativeName = lang.nativeName;
  }

  closeEditLanguage(): void {
    this.editingLanguage.set(null);
    this.selectedTranslationFile = null;
  }

  submitEditLanguage(): void {
    const lang = this.editingLanguage();
    if (!lang) return;
    const name = this.editLangName.trim();
    const nativeName = this.editLangNativeName.trim();
    if (!name || !nativeName) return;

    this.languageSaving.set(true);
    const req: UpdateLanguageRequest = { name, nativeName };
    this.adminSvc.updateLanguage(lang.code, req).subscribe({
      next: updated => {
        this.languages.update(list => list.map(l => l.code === lang.code ? updated : l));

        if (this.selectedTranslationFile) {
          this.adminSvc.uploadTranslation(lang.code, this.selectedTranslationFile).subscribe({
            next: () => {
              this.languages.update(list => list.map(l => l.code === lang.code ? { ...l, hasTranslation: true } : l));
              this.languageSaving.set(false);
              this.closeEditLanguage();
              this.toast.success(`Language '${lang.code}' updated with new translation.`);
            },
            error: () => {
              this.languageSaving.set(false);
              this.toast.error('Metadata saved but translation upload failed.');
            },
          });
        } else {
          this.languageSaving.set(false);
          this.closeEditLanguage();
          this.toast.success(`Language '${lang.code}' updated.`);
        }
      },
      error: () => {
        this.languageSaving.set(false);
        this.toast.error('Failed to update language.');
      },
    });
  }

  onTranslationFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedTranslationFile = input.files?.[0] ?? null;
  }

  toggleLanguage(lang: AdminLanguageDto): void {
    this.adminSvc.toggleLanguage(lang.code).subscribe({
      next: () => {
        this.languages.update(list => list.map(l =>
          l.code === lang.code ? { ...l, isActive: !l.isActive } : l
        ));
        this.toast.success(`Language '${lang.code}' ${lang.isActive ? 'deactivated' : 'activated'}.`);
      },
      error: () => this.toast.error('Failed to toggle language.'),
    });
  }

  confirmDeleteLanguage(lang: AdminLanguageDto): void {
    this.confirmMsg.set(`Delete language '${lang.code} — ${lang.name}'? This cannot be undone.`);
    this.confirmCallback = () => {
      this.adminSvc.deleteLanguage(lang.code).subscribe({
        next: () => {
          this.languages.update(list => list.filter(l => l.code !== lang.code));
          this.toast.success(`Language '${lang.code}' deleted.`);
        },
        error: () => this.toast.error('Failed to delete language.'),
      });
    };
  }

  // ── Confirm dialog ────────────────────────────────────────────────────

  executeConfirm(): void {
    this.confirmCallback?.();
    this.cancelConfirm();
  }

  cancelConfirm(): void {
    this.confirmMsg.set('');
    this.confirmCallback = undefined;
  }
}
