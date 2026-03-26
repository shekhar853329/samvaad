import { Injectable, signal } from '@angular/core';

export type Theme = 'light' | 'dark' | 'system';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly _theme = signal<Theme>('system');
  readonly theme = this._theme.asReadonly();

  private readonly mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

  constructor() {
    const saved = (localStorage.getItem('pref:theme') ?? 'system') as Theme;
    this._theme.set(saved);
    this.applyTheme();
    this.mediaQuery.addEventListener('change', () => this.applyTheme());
  }

  setTheme(theme: Theme): void {
    this._theme.set(theme);
    localStorage.setItem('pref:theme', theme);
    this.applyTheme();
  }

  private applyTheme(): void {
    const theme = this._theme();
    const isDark = theme === 'dark' || (theme === 'system' && this.mediaQuery.matches);
    document.documentElement.classList.toggle('dark', isDark);
  }
}
