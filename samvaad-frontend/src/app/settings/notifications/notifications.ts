import { Component, OnInit, signal, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SettingsService } from '../../core/services/settings.service';
import { NotificationPrefs } from '../../core/models/settings.models';

@Component({
  selector: 'app-settings-notifications',
  imports: [FormsModule],
  templateUrl: './notifications.html'
})
export class SettingsNotifications implements OnInit {
  private settingsService = inject(SettingsService);
  prefs = signal<NotificationPrefs | null>(null);
  loading = signal(true);
  saving = signal(false);
  saved = signal(false);
  success = signal('');
  error = signal('');

  ngOnInit(): void {
    this.settingsService.getNotificationPrefs().subscribe({
      next: p => { this.prefs.set(p); this.loading.set(false); },
      error: () => { this.error.set('Could not load preferences.'); this.loading.set(false); }
    });
  }

  set<K extends keyof NotificationPrefs>(key: K, value: NotificationPrefs[K]): void {
    const p = this.prefs();
    if (p) this.prefs.set({ ...p, [key]: value });
  }

  save(): void {
    const p = this.prefs();
    if (!p) return;
    this.saving.set(true);
    this.error.set('');
    this.settingsService.saveNotificationPrefs(p).subscribe({
      next: updated => {
        this.prefs.set(updated);
        this.saving.set(false);
        this.saved.set(true);
        setTimeout(() => this.saved.set(false), 3000);
      },
      error: err => {
        this.error.set(err?.error?.message ?? 'Failed to save preferences.');
        this.saving.set(false);
      }
    });
  }
}
