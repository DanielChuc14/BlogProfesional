import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '../../core/pipes/translate.pipe';

@Component({
  selector: 'app-not-found',
  standalone: true,
  templateUrl: './not-found.component.html',
  imports: [RouterLink, TranslatePipe],
})
export class NotFoundComponent {}
