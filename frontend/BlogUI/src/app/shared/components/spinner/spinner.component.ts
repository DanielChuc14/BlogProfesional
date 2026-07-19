import { Component, input } from '@angular/core';

@Component({
  selector: 'app-spinner',
  standalone: true,
  templateUrl: './spinner.component.html',
})
export class SpinnerComponent {
  readonly size = input<'sm' | 'md' | 'lg'>('md');
  readonly wrapperClass = input<string>('p-8');

  sizeClass(): string {
    const map = { sm: 'w-4 h-4', md: 'w-8 h-8', lg: 'w-12 h-12' };
    return map[this.size()];
  }
}
