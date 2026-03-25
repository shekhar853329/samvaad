import { Component, OnInit, signal, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SettingsService } from '../../core/services/settings.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-settings-account',
  imports: [FormsModule],
  templateUrl: './account.html'
})
export class SettingsAccount implements OnInit {
  private settingsService = inject(SettingsService);
  private authService = inject(AuthService);

  email = signal('');
  newEmail = signal('');
  currentPwdForEmail = signal('');
  currentPwd = signal('');
  newPwd = signal('');
  confirmPwd = signal('');

  savingEmail = signal(false);
  savingPwd = signal(false);
  emailSuccess = signal('');
  emailError = signal('');
  pwdSuccess = signal('');
  pwdError = signal('');

  ngOnInit(): void {
    // Pre-fill current email from cached user
    const user = this.authService.user();
    if (user) this.email.set(user.email ?? '');
  }

  saveEmail(): void {
    if (!this.newEmail().trim() || !this.currentPwdForEmail()) {
      this.emailError.set('Please fill in all email fields.');
      return;
    }
    this.savingEmail.set(true);
    this.emailError.set('');
    this.emailSuccess.set('');
    this.settingsService.updateEmail(this.newEmail().trim(), this.currentPwdForEmail()).subscribe({
      next: () => {
        this.email.set(this.newEmail().trim());
        this.newEmail.set('');
        this.currentPwdForEmail.set('');
        this.savingEmail.set(false);
        this.emailSuccess.set('Email updated successfully.');
        setTimeout(() => this.emailSuccess.set(''), 3500);
      },
      error: err => {
        this.emailError.set(err?.error?.message ?? 'Failed to update email.');
        this.savingEmail.set(false);
      }
    });
  }

  savePassword(): void {
    if (!this.currentPwd() || !this.newPwd()) {
      this.pwdError.set('Please fill in all password fields.');
      return;
    }
    if (this.newPwd().length < 8) {
      this.pwdError.set('New password must be at least 8 characters.');
      return;
    }
    if (this.newPwd() !== this.confirmPwd()) {
      this.pwdError.set('New passwords do not match.');
      return;
    }
    this.savingPwd.set(true);
    this.pwdError.set('');
    this.pwdSuccess.set('');
    this.settingsService.updatePassword(this.currentPwd(), this.newPwd()).subscribe({
      next: () => {
        this.currentPwd.set('');
        this.newPwd.set('');
        this.confirmPwd.set('');
        this.savingPwd.set(false);
        this.pwdSuccess.set('Password changed successfully.');
        setTimeout(() => this.pwdSuccess.set(''), 3500);
      },
      error: err => {
        this.pwdError.set(err?.error?.message ?? 'Failed to change password.');
        this.savingPwd.set(false);
      }
    });
  }
}
