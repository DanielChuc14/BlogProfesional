import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { PostSummaryDto } from '../../../core/models';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';

@Component({
  selector: 'app-post-card',
  standalone: true,
  templateUrl: './post-card.component.html',
  imports: [RouterLink, DatePipe, TranslatePipe],
})
export class PostCardComponent {
  readonly post = input.required<PostSummaryDto>();
}
