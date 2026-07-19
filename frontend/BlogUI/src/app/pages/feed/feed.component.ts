import { Component, inject, signal, OnInit } from '@angular/core';
import { PostService } from '../../core/services/post.service';
import { PostSummaryDto } from '../../core/models';
import { PostCardComponent } from '../../shared/components/post-card/post-card.component';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { TranslatePipe } from '../../core/pipes/translate.pipe';

@Component({
  selector: 'app-feed',
  standalone: true,
  templateUrl: './feed.component.html',
  imports: [PostCardComponent, SpinnerComponent, TranslatePipe],
})
export class FeedComponent implements OnInit {
  private readonly postSvc = inject(PostService);

  readonly posts      = signal<PostSummaryDto[]>([]);
  readonly loading    = signal(true);
  readonly loadingMore = signal(false);
  readonly hasMore    = signal(false);
  private cursor: string | null = null;

  ngOnInit(): void {
    this.fetch(true);
  }

  loadMore(): void {
    this.fetch(false);
  }

  private fetch(reset: boolean): void {
    if (reset) { this.loading.set(true); this.cursor = null; }
    else { this.loadingMore.set(true); }

    this.postSvc.getPersonalizedFeed(this.cursor ?? undefined).subscribe({
      next: res => {
        if (reset) this.posts.set(res.items);
        else this.posts.update(p => [...p, ...res.items]);
        this.cursor = res.nextCursor;
        this.hasMore.set(res.hasMore);
        this.loading.set(false);
        this.loadingMore.set(false);
      },
      error: () => { this.loading.set(false); this.loadingMore.set(false); },
    });
  }
}
