import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ThemeService, Theme } from '../../core/services/theme.service';
import { FontService, FontFamily, FontSize, FontWeight, LineHeight } from '../../core/services/font.service';
import { AccentService, AccentColor, ACCENT_COLORS } from '../../core/services/accent.service';

@Component({
  selector: 'app-settings-appearance',
  imports: [FormsModule],
  templateUrl: './appearance.html'
})
export class SettingsAppearance implements OnInit {
  private themeService  = inject(ThemeService);
  private fontService   = inject(FontService);
  private accentService = inject(AccentService);

  theme      = this.themeService.theme;
  fontFamily = this.fontService.fontFamily;
  fontSize   = this.fontService.fontSize;
  fontWeight = this.fontService.fontWeight;
  lineHeight = this.fontService.lineHeight;
  accent     = this.accentService.accent;

  readonly accentColors = ACCENT_COLORS;

  compactMode  = signal(false);
  showPreviews = signal(true);
  reduceMotion = signal(false);
  highContrast = signal(false);
  saved        = signal(false);

  ngOnInit(): void {
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

  selectFontSize(size: string): void {
    this.fontService.setFontSize(size as FontSize);
  }

  selectFontWeight(weight: string): void {
    this.fontService.setFontWeight(weight as FontWeight);
  }

  selectLineHeight(lh: string): void {
    this.fontService.setLineHeight(lh as LineHeight);
  }

  selectAccent(color: AccentColor): void {
    this.accentService.setAccent(color);
  }

  save(): void {
    localStorage.setItem('pref:compactMode',  String(this.compactMode()));
    localStorage.setItem('pref:showPreviews', String(this.showPreviews()));
    localStorage.setItem('pref:reduceMotion', String(this.reduceMotion()));
    localStorage.setItem('pref:highContrast', String(this.highContrast()));
    this.saved.set(true);
    setTimeout(() => this.saved.set(false), 3000);
  }

  reset(): void {
    this.themeService.setTheme('system');
    this.fontService.setFont('Inter');
    this.fontService.setFontSize('m');
    this.fontService.setFontWeight('Regular (400)');
    this.fontService.setLineHeight('Normal (1.6)');
    this.accentService.setAccent(ACCENT_COLORS[0]);
    this.compactMode.set(false);
    this.showPreviews.set(true);
    this.reduceMotion.set(false);
    this.highContrast.set(false);
  }
}
