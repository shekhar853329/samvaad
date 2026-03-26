import { Injectable, signal } from '@angular/core';

export type FontFamily = 'Inter' | 'Roboto' | 'Poppins' | 'Merriweather' | 'Georgia' | 'Monospace';

const FONT_STACK: Record<FontFamily, string> = {
  'Inter':        "'Inter', -apple-system, BlinkMacSystemFont, sans-serif",
  'Roboto':       "'Roboto', sans-serif",
  'Poppins':      "'Poppins', sans-serif",
  'Merriweather': "'Merriweather', Georgia, serif",
  'Georgia':      "Georgia, 'Times New Roman', serif",
  'Monospace':    "ui-monospace, 'Cascadia Code', 'Source Code Pro', Menlo, Consolas, monospace",
};

@Injectable({ providedIn: 'root' })
export class FontService {
  private readonly _fontFamily = signal<FontFamily>('Inter');
  readonly fontFamily = this._fontFamily.asReadonly();

  constructor() {
    const saved = (localStorage.getItem('pref:fontFamily') ?? 'Inter') as FontFamily;
    this._fontFamily.set(saved);
    this.applyFont();
  }

  setFont(font: FontFamily): void {
    this._fontFamily.set(font);
    localStorage.setItem('pref:fontFamily', font);
    this.applyFont();
  }

  private applyFont(): void {
    const stack = FONT_STACK[this._fontFamily()] ?? FONT_STACK['Inter'];
    document.documentElement.style.setProperty('--font-sans', stack);
  }
}
