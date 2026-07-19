import { Component, inject, input, output, signal } from '@angular/core';
import { ImageCropperComponent, ImageCroppedEvent } from 'ngx-image-cropper';

@Component({
  selector: 'app-image-cropper-modal',
  standalone: true,
  imports: [ImageCropperComponent],
  template: `
    <div class="fixed inset-0 z-50 flex items-center justify-center bg-black/70 p-4">
      <div class="bg-white rounded-2xl shadow-2xl w-full max-w-lg flex flex-col gap-4 p-6">
        <h2 class="text-lg font-semibold text-gray-900">{{ title() }}</h2>

        <div class="border border-gray-200 rounded-xl overflow-hidden bg-gray-50" style="max-height: 380px;">
          <image-cropper
            [imageFile]="imageFile()"
            [maintainAspectRatio]="true"
            [aspectRatio]="aspectRatio()"
            [resizeToWidth]="resizeToWidth()"
            format="jpeg"
            (imageCropped)="onCropped($event)"
            (imageLoaded)="imageLoaded.set(true)"
            (loadImageFailed)="imageLoaded.set(false)"
            style="max-height: 380px;"
          />
        </div>

        @if (!imageLoaded()) {
          <p class="text-sm text-gray-400 text-center">Loading image...</p>
        }

        <div class="flex justify-end gap-3 mt-2">
          <button class="btn btn-secondary btn-sm" (click)="cancelled.emit()">Cancel</button>
          <button class="btn btn-primary btn-sm"
                  [disabled]="!croppedBlob() || !imageLoaded()"
                  (click)="confirm()">
            Apply crop
          </button>
        </div>
      </div>
    </div>
  `,
})
export class ImageCropperModalComponent {
  readonly imageFile    = input.required<File>();
  readonly aspectRatio  = input<number>(1 / 1);
  readonly resizeToWidth = input<number>(512);
  readonly title        = input<string>('Adjust image');

  readonly cropped  = output<Blob>();
  readonly cancelled = output<void>();

  readonly imageLoaded = signal(false);
  readonly croppedBlob = signal<Blob | null>(null);

  onCropped(event: ImageCroppedEvent): void {
    this.croppedBlob.set(event.blob ?? null);
  }

  confirm(): void {
    const blob = this.croppedBlob();
    if (blob) this.cropped.emit(blob);
  }
}
