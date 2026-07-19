import { Component, inject, signal, OnInit, input } from '@angular/core';
import { TagService } from '../../core/services/tag.service';
import { PostSummaryDto } from '../../core/models';
import { PostCardComponent } from '../../shared/components/post-card/post-card.component';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { TranslatePipe } from '../../core/pipes/translate.pipe';

@Component({
  selector: 'app-tag',
  standalone: true,
  templateUrl: './tag.component.html',
  imports: [PostCardComponent, SpinnerComponent, TranslatePipe],
})
export class TagComponent implements OnInit {
  readonly slug = input.required<string>();

  private readonly tagSvc = inject(TagService);

  readonly posts       = signal<PostSummaryDto[]>([]);
  readonly loading     = signal(true);
  readonly loadingMore = signal(false);
  readonly hasMore     = signal(false);
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

    this.tagSvc.getPostsByTag(this.slug(), this.cursor ?? undefined).subscribe({
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
