import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ThemeService, Theme } from '../../core/services/theme.service';
import { FontService, FontFamily } from '../../core/services/font.service';

type FontSize = 'xs' | 's' | 'm' | 'l' | 'xl';

@Component({
  selector: 'app-settings-appearance',
  imports: [FormsModule],
  templateUrl: './appearance.html'
})
export class SettingsAppearance implements OnInit {
  private themeService = inject(ThemeService);
  private fontService = inject(FontService);

  theme = this.themeService.theme;
  fontFamily = this.fontService.fontFamily;
  fontSize = signal<FontSize>('m');
  fontWeight = signal('Regular (400)');
  lineHeight = signal('Normal (1.6)');
  compactMode = signal(false);
  showPreviews = signal(true);
  reduceMotion = signal(false);
  highContrast = signal(false);
  saved = signal(false);

  ngOnInit(): void {
    this.fontSize.set((localStorage.getItem('pref:fontSize') ?? 'm') as FontSize);
    this.fontWeight.set(localStorage.getItem('pref:fontWeight') ?? 'Regular (400)');
    this.lineHeight.set(localStorage.getItem('pref:lineHeight') ?? 'Normal (1.6)');
    this.compactMode.set(localStorage.getItem('pref:compactMode') === 'true');
    this.showPreviews.set(localStorage.getItem('pref:showPreviews') !== 'false');
    this.reduceMotion.set(localStorage.getItem('pref:reduceMotion') === 'true');
    this.highContrast.set(localStorage.getItem('pref:highContrast') === 'true');
  }

  selectTheme(theme: Theme): void {
    this.themeService.setTheme(theme);
  }

  selectFont(font: string): void {
    this.fontService.setFont(font as FontFamily);
  }

  save(): void {
    localStorage.setItem('pref:fontSize', this.fontSize());
    localStorage.setItem('pref:fontWeight', this.fontWeight());
    localStorage.setItem('pref:lineHeight', this.lineHeight());
    localStorage.setItem('pref:compactMode', String(this.compactMode()));
    localStorage.setItem('pref:showPreviews', String(this.showPreviews()));
    localStorage.setItem('pref:reduceMotion', String(this.reduceMotion()));
    localStorage.setItem('pref:highContrast', String(this.highContrast()));
    this.saved.set(true);
    setTimeout(() => this.saved.set(false), 3000);
  }

  reset(): void {
    this.themeService.setTheme('system');
    this.fontService.setFont('Inter');
    this.fontSize.set('m');
    this.fontWeight.set('Regular (400)');
    this.lineHeight.set('Normal (1.6)');
    this.compactMode.set(false);
    this.showPreviews.set(true);
    this.reduceMotion.set(false);
    this.highContrast.set(false);
  }
}
