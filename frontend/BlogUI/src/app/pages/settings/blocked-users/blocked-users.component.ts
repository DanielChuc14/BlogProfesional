import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BlockService } from '../../../core/services/block.service';
import { ToastService } from '../../../core/services/toast.service';
import { BlockedUserDto } from '../../../core/models';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-blocked-users',
  standalone: true,
  templateUrl: './blocked-users.component.html',
  imports: [RouterLink, SpinnerComponent],
})
export class BlockedUsersComponent implements OnInit {
  private readonly blockSvc = inject(BlockService);
  private readonly toast    = inject(ToastService);

  readonly items       = signal<BlockedUserDto[]>([]);
  readonly loading     = signal(true);
  readonly loadingMore = signal(false);
  readonly hasMore     = signal(false);
  readonly unblocking  = signal<string | null>(null);

  private page = 1;
  private readonly PAGE_SIZE = 20;

  ngOnInit(): void {
    this.fetchPage(true);
  }

  loadMore(): void {
    this.fetchPage(false);
  }

  unblock(userId: string): void {
    this.unblocking.set(userId);
    this.blockSvc.unblock(userId).subscribe({
      next: () => {
        this.items.update(list => list.filter(u => u.userId !== userId));
        this.unblocking.set(null);
        this.toast.success('User unblocked.');
      },
      error: () => {
        this.unblocking.set(null);
        this.toast.error('Failed to unblock.');
      },
    });
  }

  private fetchPage(reset: boolean): void {
    if (reset) {
      this.page = 1;
      this.loading.set(true);
    } else {
      this.loadingMore.set(true);
    }

    this.blockSvc.getBlocked(this.page, this.PAGE_SIZE).subscribe({
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
}
