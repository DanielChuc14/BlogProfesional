import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

// Provides AES-GCM encryption using the browser's native Web Crypto API.
// The key is derived from a hardcoded app secret via SHA-256, which means
// the data is protected from casual localStorage inspection but not from
// someone with access to the source code.
@Injectable({ providedIn: 'root' })
export class EncryptionService {
  private readonly platformId = inject(PLATFORM_ID);
  private key: CryptoKey | null = null;

  // Resolves once the key is ready. Await this before calling encrypt/decrypt.
  readonly ready: Promise<void>;

  private readonly APP_SECRET = 'BlogPlatform__Storage__Key__v1__2026';

  constructor() {
    this.ready = this.deriveKey();
  }

  private async deriveKey(): Promise<void> {
    if (!isPlatformBrowser(this.platformId)) return;
    const keyBytes = await crypto.subtle.digest(
      'SHA-256',
      new TextEncoder().encode(this.APP_SECRET)
    );
    this.key = await crypto.subtle.importKey(
      'raw',
      keyBytes,
      'AES-GCM',
      false,
      ['encrypt', 'decrypt']
    );
  }

  async encrypt(plaintext: string): Promise<string> {
    await this.ready;
    if (!this.key) return plaintext;

    const iv = crypto.getRandomValues(new Uint8Array(12));
    const encoded = new TextEncoder().encode(plaintext);
    const ciphertext = await crypto.subtle.encrypt(
      { name: 'AES-GCM', iv },
      this.key,
      encoded
    );

    // Prepend IV (12 bytes) to ciphertext and encode to base64
    const combined = new Uint8Array(12 + ciphertext.byteLength);
    combined.set(iv, 0);
    combined.set(new Uint8Array(ciphertext), 12);

    let binary = '';
    for (let i = 0; i < combined.length; i++) {
      binary += String.fromCharCode(combined[i]);
    }
    return btoa(binary);
  }

  async decrypt(data: string): Promise<string | null> {
    await this.ready;
    if (!this.key) return data;

    try {
      const binary = atob(data);
      const combined = new Uint8Array(binary.length);
      for (let i = 0; i < binary.length; i++) {
        combined[i] = binary.charCodeAt(i);
      }

      const iv = combined.slice(0, 12);
      const ciphertext = combined.slice(12);

      const decrypted = await crypto.subtle.decrypt(
        { name: 'AES-GCM', iv },
        this.key,
        ciphertext
      );

      return new TextDecoder().decode(decrypted);
    } catch {
      return null;
    }
  }
}
