import { Injectable, signal } from '@angular/core';

export interface AccentColor {
  hex:   string;
  dark:  string;
  label: string;
}

export const ACCENT_COLORS: AccentColor[] = [
  { hex: '#534AB7', dark: '#3C3489', label: 'Purple'  },
  { hex: '#0F6E56', dark: '#0A4F3E', label: 'Teal'    },
  { hex: '#185FA5', dark: '#124B84', label: 'Blue'     },
  { hex: '#993C1D', dark: '#7A3017', label: 'Coral'    },
  { hex: '#BA7517', dark: '#956011', label: 'Amber'    },
  { hex: '#3B6D11', dark: '#2D540D', label: 'Green'    },
  { hex: '#993556', dark: '#7A2A44', label: 'Pink'     },
];

@Injectable({ providedIn: 'root' })
export class AccentService {
  private readonly _accent = signal<AccentColor>(ACCENT_COLORS[0]);
  readonly accent = this._accent.asReadonly();

  constructor() {
    const savedHex = localStorage.getItem('pref:accentColor');
    const match = ACCENT_COLORS.find(c => c.hex === savedHex);
    if (match) this._accent.set(match);
    this.applyAccent();
  }

  setAccent(color: AccentColor): void {
    this._accent.set(color);
    localStorage.setItem('pref:accentColor', color.hex);
    this.applyAccent();
  }

  private applyAccent(): void {
    const { hex, dark } = this._accent();
    const el = document.documentElement;
    el.style.setProperty('--color-accent',      hex);
    el.style.setProperty('--color-accent-dark',  dark);
    el.style.setProperty('--color-accent-rgb',      this.hexToRgb(hex));
    el.style.setProperty('--color-accent-dark-rgb',  this.hexToRgb(dark));
  }

  private hexToRgb(hex: string): string {
    const r = parseInt(hex.slice(1, 3), 16);
    const g = parseInt(hex.slice(3, 5), 16);
    const b = parseInt(hex.slice(5, 7), 16);
    return `${r} ${g} ${b}`;
  }
}
