import { Injectable, signal } from '@angular/core';

export type FontFamily = 'Inter' | 'Roboto' | 'Poppins' | 'Merriweather' | 'Georgia' | 'Monospace';
export type FontSize   = 'xs' | 's' | 'm' | 'l' | 'xl';
export type FontWeight = 'Light (300)' | 'Regular (400)' | 'Medium (500)';
export type LineHeight = 'Compact (1.4)' | 'Normal (1.6)' | 'Relaxed (1.8)' | 'Spacious (2.0)';

const FONT_STACK: Record<FontFamily, string> = {
  'Inter':        "'Inter', -apple-system, BlinkMacSystemFont, sans-serif",
  'Roboto':       "'Roboto', sans-serif",
  'Poppins':      "'Poppins', sans-serif",
  'Merriweather': "'Merriweather', Georgia, serif",
  'Georgia':      "Georgia, 'Times New Roman', serif",
  'Monospace':    "ui-monospace, 'Cascadia Code', 'Source Code Pro', Menlo, Consolas, monospace",
};

const FONT_SIZE_MAP: Record<FontSize, string> = {
  xs: '11px',
  s:  '13px',
  m:  '14px',
  l:  '16px',
  xl: '18px',
};

const FONT_WEIGHT_MAP: Record<FontWeight, string> = {
  'Light (300)':   '300',
  'Regular (400)': '400',
  'Medium (500)':  '500',
};

const LINE_HEIGHT_MAP: Record<LineHeight, string> = {
  'Compact (1.4)':  '1.4',
  'Normal (1.6)':   '1.6',
  'Relaxed (1.8)':  '1.8',
  'Spacious (2.0)': '2.0',
};

@Injectable({ providedIn: 'root' })
export class FontService {
  private readonly _fontFamily  = signal<FontFamily>('Inter');
  private readonly _fontSize    = signal<FontSize>('m');
  private readonly _fontWeight  = signal<FontWeight>('Regular (400)');
  private readonly _lineHeight  = signal<LineHeight>('Normal (1.6)');

  readonly fontFamily = this._fontFamily.asReadonly();
  readonly fontSize   = this._fontSize.asReadonly();
  readonly fontWeight = this._fontWeight.asReadonly();
  readonly lineHeight = this._lineHeight.asReadonly();

  constructor() {
    this._fontFamily.set((localStorage.getItem('pref:fontFamily') ?? 'Inter') as FontFamily);
    this._fontSize.set((localStorage.getItem('pref:fontSize')    ?? 'm')    as FontSize);
    this._fontWeight.set((localStorage.getItem('pref:fontWeight') ?? 'Regular (400)') as FontWeight);
    this._lineHeight.set((localStorage.getItem('pref:lineHeight') ?? 'Normal (1.6)')  as LineHeight);
    this.applyAll();
  }

  setFont(font: FontFamily): void {
    this._fontFamily.set(font);
    localStorage.setItem('pref:fontFamily', font);
    document.documentElement.style.setProperty('--font-sans', FONT_STACK[font] ?? FONT_STACK['Inter']);
  }

  setFontSize(size: FontSize): void {
    this._fontSize.set(size);
    localStorage.setItem('pref:fontSize', size);
    document.documentElement.style.setProperty('--app-font-size', FONT_SIZE_MAP[size]);
  }

  setFontWeight(weight: FontWeight): void {
    this._fontWeight.set(weight);
    localStorage.setItem('pref:fontWeight', weight);
    document.documentElement.style.setProperty('--app-font-weight', FONT_WEIGHT_MAP[weight]);
  }

  setLineHeight(lh: LineHeight): void {
    this._lineHeight.set(lh);
    localStorage.setItem('pref:lineHeight', lh);
    document.documentElement.style.setProperty('--app-line-height', LINE_HEIGHT_MAP[lh]);
  }

  private applyAll(): void {
    const el = document.documentElement;
    el.style.setProperty('--font-sans',        FONT_STACK[this._fontFamily()]   ?? FONT_STACK['Inter']);
    el.style.setProperty('--app-font-size',    FONT_SIZE_MAP[this._fontSize()]);
    el.style.setProperty('--app-font-weight',  FONT_WEIGHT_MAP[this._fontWeight()]);
    el.style.setProperty('--app-line-height',  LINE_HEIGHT_MAP[this._lineHeight()]);
  }
}
