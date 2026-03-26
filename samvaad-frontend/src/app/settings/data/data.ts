import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-settings-data',
  imports: [FormsModule],
  templateUrl: './data.html'
})
export class SettingsData implements OnInit {
  mediaQuality = signal('High (original)');
  toast = signal('');
  savedQuality = signal(false);

  ngOnInit(): void {
    this.mediaQuality.set(localStorage.getItem('pref:mediaQuality') ?? 'High (original)');
  }

  private showToast(msg: string): void {
    this.toast.set(msg);
    setTimeout(() => this.toast.set(''), 3500);
  }

  clearCache(): void {
    const keys = Object.keys(localStorage).filter(k => k.startsWith('pref:'));
    keys.forEach(k => localStorage.removeItem(k));
    this.showToast('Cache cleared successfully.');
  }

  requestExport(type: string): void {
    this.showToast(`${type} export requested. You will be emailed when it is ready.`);
  }

  saveQuality(): void {
    localStorage.setItem('pref:mediaQuality', this.mediaQuality());
    this.savedQuality.set(true);
    setTimeout(() => this.savedQuality.set(false), 3000);
  }
}
