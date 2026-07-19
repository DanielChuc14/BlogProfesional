import { Component, inject, signal, OnInit, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProfileService } from '../../../core/services/profile.service';
import { FollowerDto } from '../../../core/models';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';

@Component({
  selector: 'app-follow-list',
  standalone: true,
  templateUrl: './follow-list.component.html',
  imports: [RouterLink, SpinnerComponent, TranslatePipe],
})
export class FollowListComponent implements OnInit {
  readonly username = input.required<string>();
  readonly listType = input.required<'followers' | 'following'>();

  private readonly profileSvc = inject(ProfileService);

  readonly items       = signal<FollowerDto[]>([]);
  readonly loading     = signal(true);
  readonly loadingMore = signal(false);
  readonly hasMore     = signal(false);

  private slug = '';
  private page = 1;
  private readonly PAGE_SIZE = 20;

  ngOnInit(): void {
    this.profileSvc.getProfile(this.username()).subscribe({
      next: profile => {
        this.slug = profile.slug;
        this.fetchPage(true);
      },
      error: () => this.loading.set(false),
    });
  }

  loadMore(): void {
    this.fetchPage(false);
  }

  private fetchPage(reset: boolean): void {
    if (reset) {
      this.page = 1;
      this.loading.set(true);
    } else {
      this.loadingMore.set(true);
    }

    const call = this.listType() === 'followers'
      ? this.profileSvc.getFollowers(this.slug, this.page, this.PAGE_SIZE)
      : this.profileSvc.getFollowing(this.slug, this.page, this.PAGE_SIZE);

    call.subscribe({
      next: result => {
        if (reset) {
          this.items.set(result);
        } else {
          this.items.update(prev => [...prev, ...result]);
        }
        this.hasMore.set(result.length === this.PAGE_SIZE);
        this.page++;
        this.loading.set(false);
        this.loadingMore.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.loadingMore.set(false);
      },
    });
  }

  get pageTitle(): string {
    return this.listType() === 'followers'
      ? `Followers of @${this.username()}`
      : `Following — @${this.username()}`;
  }
}
