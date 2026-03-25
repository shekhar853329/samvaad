import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { SettingsService } from '../../core/services/settings.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-settings-danger',
  imports: [FormsModule],
  templateUrl: './danger.html'
})
export class SettingsDanger {
  private settingsService = inject(SettingsService);
  private authService = inject(AuthService);
  private router = inject(Router);

  showDeleteConfirm = signal(false);
  deletePassword = signal('');
  deleting = signal(false);
  deleteError = signal('');

  confirmDelete(): void {
    const pwd = this.deletePassword();
    if (!pwd) {
      this.deleteError.set('Please enter your password to confirm deletion.');
      return;
    }
    this.deleting.set(true);
    this.deleteError.set('');
    this.settingsService.deleteAccount(pwd).subscribe({
      next: () => {
        this.authService.logout();
        this.router.navigate(['/']);
      },
      error: err => {
        this.deleteError.set(err?.error?.message ?? 'Failed to delete account. Please check your password.');
        this.deleting.set(false);
      }
    });
  }
}
