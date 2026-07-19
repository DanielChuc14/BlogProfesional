import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PostService } from '../../core/services/post.service';
import { TagService } from '../../core/services/tag.service';
import { PostSummaryDto, TagDto } from '../../core/models';
import { PostCardComponent } from '../../shared/components/post-card/post-card.component';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { TranslatePipe } from '../../core/pipes/translate.pipe';

@Component({
  selector: 'app-home',
  standalone: true,
  templateUrl: './home.component.html',
  imports: [PostCardComponent, SpinnerComponent, RouterLink, TranslatePipe],
})
export class HomeComponent implements OnInit {
  private readonly postSvc = inject(PostService);
  private readonly tagSvc  = inject(TagService);

  readonly posts      = signal<PostSummaryDto[]>([]);
  readonly tags       = signal<TagDto[]>([]);
  readonly loading    = signal(true);
  readonly loadingMore = signal(false);
  readonly hasMore    = signal(false);
  readonly activeTag  = signal<string | null>(null);

  private cursor: string | null = null;

  ngOnInit(): void {
    this.tagSvc.getTags().subscribe(tags => this.tags.set(tags));
    this.fetchPosts(true);
  }

  selectTag(slug: string | null): void {
    this.activeTag.set(slug);
    this.cursor = null;
    this.fetchPosts(true);
  }

  loadMore(): void {
    this.fetchPosts(false);
  }

  private fetchPosts(reset: boolean): void {
    if (reset) {
      this.loading.set(true);
      this.cursor = null;
    } else {
      this.loadingMore.set(true);
    }

    this.postSvc.getFeed({
      cursor:   this.cursor ?? undefined,
      pageSize: 12,
      tag:      this.activeTag() ?? undefined,
    }).subscribe({
      next: res => {
        if (reset) {
          this.posts.set(res.items);
        } else {
          this.posts.update(existing => [...existing, ...res.items]);
        }
        this.cursor  = res.nextCursor;
        this.hasMore.set(res.hasMore);
        this.loading.set(false);
        this.loadingMore.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.loadingMore.set(false);
      },
    });
  }
}
