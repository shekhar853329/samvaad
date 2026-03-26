import { Component, OnInit, signal, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SettingsService } from '../../core/services/settings.service';
import { PrivacySettings } from '../../core/models/settings.models';

@Component({
  selector: 'app-settings-privacy',
  imports: [FormsModule],
  templateUrl: './privacy.html'
})
export class SettingsPrivacy implements OnInit {
  private settingsService = inject(SettingsService);
  settings = signal<PrivacySettings | null>(null);
  loading = signal(true);
  saving = signal(false);
  saved = signal(false);
  success = signal('');
  error = signal('');

  ngOnInit(): void {
    this.settingsService.getPrivacySettings().subscribe({
      next: s => { this.settings.set(s); this.loading.set(false); },
      error: () => { this.error.set('Could not load settings.'); this.loading.set(false); }
    });
  }

  set<K extends keyof PrivacySettings>(key: K, value: PrivacySettings[K]): void {
    const s = this.settings();
    if (s) this.settings.set({ ...s, [key]: value });
  }

  save(): void {
    const s = this.settings();
    if (!s) return;
    this.saving.set(true);
    this.error.set('');
    this.settingsService.savePrivacySettings(s).subscribe({
      next: updated => {
        this.settings.set(updated);
        this.saving.set(false);
        this.saved.set(true);
        setTimeout(() => this.saved.set(false), 3000);
      },
      error: err => {
        this.error.set(err?.error?.message ?? 'Failed to save settings.');
        this.saving.set(false);
      }
    });
  }
}
