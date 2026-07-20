import { Component, inject, signal, OnInit, input } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PostService } from '../../core/services/post.service';
import { TagService } from '../../core/services/tag.service';
import { ToastService } from '../../core/services/toast.service';
import { TagDto, PostDetailDto } from '../../core/models';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { HttpErrorResponse } from '@angular/common/http';
import { TranslatePipe } from '../../core/pipes/translate.pipe';
import { extractApiError } from '../../core/utils/api-error';

@Component({
  selector: 'app-editor',
  standalone: true,
  templateUrl: './editor.component.html',
  imports: [FormsModule, SpinnerComponent, TranslatePipe],
})
export class EditorComponent implements OnInit {
  readonly editId = input<string | undefined>(undefined);

  private readonly postSvc = inject(PostService);
  private readonly tagSvc  = inject(TagService);
  private readonly toast   = inject(ToastService);
  private readonly router  = inject(Router);

  title        = '';
  excerpt      = '';
  body         = '';
  coverImageUrl = '';
  isAdultContent = false;
  tagSearch    = '';

  readonly saving      = signal(false);
  readonly initLoading = signal(false);
  readonly errorMsg    = signal('');
  readonly selectedTags = signal<TagDto[]>([]);
  readonly tagResults   = signal<TagDto[]>([]);

  ngOnInit(): void {
    const id = this.editId();
    if (id) {
      this.initLoading.set(true);
      this.postSvc.getById(id).subscribe({
        next: post => {
          this.title = post.title;
          this.excerpt = post.excerpt ?? '';
          this.body = post.body;
          this.coverImageUrl = post.coverImageUrl ?? '';
          this.isAdultContent = post.isAdultContent ?? false;
          this.selectedTags.set(post.tags.map(t => ({ id: t.id, name: t.name, slug: t.slug, postsCount: 0 })));
          this.initLoading.set(false);
        },
        error: () => {
          this.errorMsg.set('Failed to load post.');
          this.initLoading.set(false);
        },
      });
    }
  }

  searchTags(): void {
    const q = this.tagSearch.trim();
    if (!q) { this.tagResults.set([]); return; }
    this.tagSvc.autocomplete(q).subscribe(tags => {
      const selectedIds = new Set(this.selectedTags().map(t => t.id));
      this.tagResults.set(tags.filter(t => !selectedIds.has(t.id)));
    });
  }

  addTag(tag: TagDto): void {
    this.selectedTags.update(ts => [...ts, tag]);
    this.tagResults.set([]);
    this.tagSearch = '';
  }

  removeTag(tag: TagDto): void {
    this.selectedTags.update(ts => ts.filter(t => t.id !== tag.id));
  }

  save(status: 'Draft' | 'Published'): void {
    if (!this.title.trim() || !this.body.trim()) {
      this.errorMsg.set('Title and content are required.');
      return;
    }
    this.saving.set(true);
    this.errorMsg.set('');

    const req = {
      title: this.title,
      body: this.body,
      excerpt: this.excerpt || undefined,
      coverImageUrl: this.coverImageUrl || undefined,
      isAdultContent: this.isAdultContent,
      tagIds: this.selectedTags().map(t => t.id),
    };

    const id = this.editId();
    const obs = id
      ? this.postSvc.update(id, req)
      : this.postSvc.create(req);

    obs.subscribe({
      next: (post: PostDetailDto) => {
        if (status === 'Published') {
          this.postSvc.publish(post.id).subscribe({
            next: () => {
              this.saving.set(false);
              this.toast.success('toast_postPublished');
              this.router.navigate(['/post', post.slug]);
            },
            error: (err: HttpErrorResponse) => {
              this.saving.set(false);
              this.errorMsg.set(extractApiError(err, 'toast_failedToPublishPost'));
            },
          });
        } else {
          this.saving.set(false);
          this.toast.success('toast_draftSaved');
          this.router.navigate(['/editor', post.id]);
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.errorMsg.set(extractApiError(err, 'toast_failedToSavePost'));
      },
    });
  }

  publish(): void {
    this.save('Published');
  }
}
