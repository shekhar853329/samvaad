import { Component, OnInit, signal, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { SettingsService } from '../../core/services/settings.service';
import { SessionInfo } from '../../core/models/settings.models';

@Component({
  selector: 'app-settings-security',
  imports: [DatePipe],
  templateUrl: './security.html'
})
export class SettingsSecurity implements OnInit {
  private settingsService = inject(SettingsService);
  sessions = signal<SessionInfo[]>([]);
  loading = signal(true);
  revoking = signal<string | null>(null);
  revokingAll = signal(false);
  error = signal('');
  success = signal('');

  ngOnInit(): void { this.loadSessions(); }

  loadSessions(): void {
    this.loading.set(true);
    this.settingsService.getSessions().subscribe({
      next: s => { this.sessions.set(s); this.loading.set(false); },
      error: () => { this.error.set('Could not load sessions.'); this.loading.set(false); }
    });
  }

  revoke(id: string): void {
    this.revoking.set(id);
    this.settingsService.revokeSession(id).subscribe({
      next: () => {
        this.sessions.update(s => s.filter(x => x.id !== id));
        this.revoking.set(null);
        this.success.set('Session revoked.');
        setTimeout(() => this.success.set(''), 3000);
      },
      error: () => { this.error.set('Failed to revoke session.'); this.revoking.set(null); }
    });
  }

  revokeAll(): void {
    this.revokingAll.set(true);
    this.settingsService.revokeAllOtherSessions().subscribe({
      next: () => {
        this.revokingAll.set(false);
        this.success.set('All other sessions signed out.');
        setTimeout(() => this.success.set(''), 3000);
        this.loadSessions();
      },
      error: () => { this.error.set('Failed to sign out other sessions.'); this.revokingAll.set(false); }
    });
  }
}
