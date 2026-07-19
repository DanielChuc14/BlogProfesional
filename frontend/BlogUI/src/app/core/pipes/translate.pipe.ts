import { inject, Pipe, PipeTransform } from '@angular/core';
import { TranslationService } from '../services/translation.service';

@Pipe({ name: 'translate', standalone: true, pure: false })
export class TranslatePipe implements PipeTransform {
  private readonly t = inject(TranslationService);

  transform(key: string): string {
    return this.t.translate(key);
  }
}
