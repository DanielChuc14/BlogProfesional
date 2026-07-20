import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TagService } from '../../core/services/tag.service';
import { PostSummaryDto } from '../../core/models';
import { PostCardComponent } from '../../shared/components/post-card/post-card.component';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { TranslatePipe } from '../../core/pipes/translate.pipe';

@Component({
  selector: 'app-search',
  standalone: true,
  templateUrl: './search.component.html',
  imports: [PostCardComponent, SpinnerComponent, FormsModule, TranslatePipe],
})
export class SearchComponent implements OnInit {
  private readonly route   = inject(ActivatedRoute);
  private readonly router  = inject(Router);
  private readonly tagSvc  = inject(TagService);

  readonly posts      = signal<PostSummaryDto[]>([]);
  readonly loading    = signal(false);
  readonly hasMore    = signal(false);
  readonly query      = signal('');
  // Sin esto, un fallo del API dejaba la lista vacia y la UI lo mostraba
  // como "sin resultados", indistinguible de una busqueda legitima sin coincidencias.
  readonly failed     = signal(false);

  searchInput = '';
  private cursor: string | undefined;

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const q = (params['q'] ?? '').trim();
      this.query.set(q);
      this.searchInput = q;
      if (q) {
        this.cursor = undefined;
        this.posts.set([]);
        this.loadResults(q);
      }
    });
  }

  private loadResults(q: string, append = false): void {
    this.loading.set(true);
    this.failed.set(false);
    this.tagSvc.search(q, undefined, this.cursor).subscribe({
      next: res => {
        this.posts.update(current => append ? [...current, ...res.items] : res.items);
        this.hasMore.set(res.hasMore);
        this.cursor = res.nextCursor ?? undefined;
        this.loading.set(false);
      },
      error: () => {
        this.failed.set(true);
        this.hasMore.set(false);
        this.loading.set(false);
      },
    });
  }

  loadMore(): void {
    const q = this.query();
    if (!q || !this.hasMore()) return;
    this.loadResults(q, true);
  }

  submitSearch(): void {
    const q = this.searchInput.trim();
    if (!q) return;
    this.router.navigate(['/search'], { queryParams: { q } });
  }
}
