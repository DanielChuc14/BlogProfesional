import { Component, inject, signal, OnInit, OnDestroy, input, ElementRef, PLATFORM_ID } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe, isPlatformBrowser } from '@angular/common';
import { PostService } from '../../../core/services/post.service';
import { CommentService } from '../../../core/services/comment.service';
import { ProfileService } from '../../../core/services/profile.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { PostDetailDto, CommentDto, BlogThemeDto } from '../../../core/models';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { ReportModalComponent } from '../../../shared/components/report-modal/report-modal.component';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';

@Component({
  selector: 'app-post-detail',
  standalone: true,
  templateUrl: './post-detail.component.html',
  imports: [RouterLink, DatePipe, SpinnerComponent, FormsModule, TranslatePipe, ReportModalComponent],
})
export class PostDetailComponent implements OnInit, OnDestroy {
  readonly slug = input.required<string>();

  private readonly postSvc    = inject(PostService);
  private readonly commentSvc = inject(CommentService);
  private readonly profileSvc = inject(ProfileService);
  private readonly toast      = inject(ToastService);
  readonly auth               = inject(AuthService);
  private readonly elRef      = inject(ElementRef);
  private readonly platformId = inject(PLATFORM_ID);

  readonly post      = signal<PostDetailDto | null>(null);
  readonly comments  = signal<CommentDto[]>([]);
  readonly loading   = signal(true);
  readonly liked     = signal(false);
  readonly submitting = signal(false);
  readonly showAdultWarning   = signal(false);
  readonly reportTarget       = signal<{ type: 'Post' | 'Comment'; id: string } | null>(null);
  newComment = '';

  ngOnInit(): void {
    this.postSvc.getBySlug(this.slug()).subscribe({
      next: post => {
        this.post.set(post);
        this.loading.set(false);
        if (post.isAdultContent) {
          this.showAdultWarning.set(true);
        }
        this.commentSvc.getByPost(post.id).subscribe(c => this.comments.set(c));
        this.profileSvc.getProfile(post.author.username).subscribe({
          next: p => this.applyTheme(p.theme),
        });
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

  acceptAdultContent(): void {
    this.showAdultWarning.set(false);
  }

  toggleLike(): void {
    if (!this.auth.isLoggedIn()) { this.toast.info('Sign in to like posts.'); return; }
    const post = this.post();
    if (!post) return;
    if (this.liked()) {
      this.postSvc.unlike(post.id).subscribe(() => this.liked.set(false));
    } else {
      this.postSvc.like(post.id).subscribe(() => this.liked.set(true));
    }
  }

  openReport(type: 'Post' | 'Comment', id: string): void {
    this.reportTarget.set({ type, id });
  }

  closeReport(): void {
    this.reportTarget.set(null);
  }

  submitComment(): void {
    const post = this.post();
    if (!post || !this.newComment.trim()) return;
    this.submitting.set(true);
    this.commentSvc.create(post.id, { body: this.newComment.trim() }).subscribe({
      next: comment => {
        this.comments.update(cs => [comment, ...cs]);
        this.newComment = '';
        this.submitting.set(false);
        this.toast.success('Comment posted!');
      },
      error: (err: HttpErrorResponse) => {
        this.submitting.set(false);
        this.toast.error(err.error?.error ?? err.error?.title ?? 'Failed to post comment.');
      },
    });
  }
}
