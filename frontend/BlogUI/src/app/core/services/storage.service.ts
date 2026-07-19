import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { EncryptionService } from './encryption.service';

// Typed, encrypted localStorage wrapper.
// All values are serialized to JSON and encrypted with AES-GCM before storage.
// getItem returns null if the key doesn't exist or decryption fails.
@Injectable({ providedIn: 'root' })
export class StorageService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly enc        = inject(EncryptionService);

  async setItem<T>(key: string, value: T): Promise<void> {
    if (!isPlatformBrowser(this.platformId)) return;
    const encrypted = await this.enc.encrypt(JSON.stringify(value));
    localStorage.setItem(key, encrypted);
  }

  async getItem<T>(key: string): Promise<T | null> {
    if (!isPlatformBrowser(this.platformId)) return null;
    const raw = localStorage.getItem(key);
    if (!raw) return null;
    const plain = await this.enc.decrypt(raw);
    if (!plain) return null;
    try {
      return JSON.parse(plain) as T;
    } catch {
      return null;
    }
  }

  removeItem(key: string): void {
    if (!isPlatformBrowser(this.platformId)) return;
    localStorage.removeItem(key);
  }

  clear(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    localStorage.clear();
  }
}
