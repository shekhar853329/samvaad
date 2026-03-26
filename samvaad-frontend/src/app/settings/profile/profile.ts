import { Component, OnInit, signal, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../core/services/user.service';
import { UserProfile } from '../../core/models/user.models';

@Component({
  selector: 'app-settings-profile',
  imports: [FormsModule],
  templateUrl: './profile.html'
})
export class SettingsProfile implements OnInit {
  private userService = inject(UserService);

  loading = signal(true);
  saving = signal(false);
  saved = signal(false);
  avatarUploading = signal(false);
  successMsg = signal('');
  errorMsg = signal('');

  // Form fields
  username = signal('');
  displayName = signal('');
  bio = signal('');
  website = signal('');
  location = signal('');
  avatarUrl = signal<string | null>(null);
  tags = signal<string[]>([]);
  newTag = signal('');

  // Original data (for cancel)
  private _original: Partial<UserProfile> = {};

  ngOnInit(): void {
    this.loadProfile();
  }

  private loadProfile(): void {
    this.loading.set(true);
    this.userService.getMyProfile().subscribe({
      next: profile => {
        this._original = profile;
        this.username.set(profile.username);
        this.displayName.set(profile.displayName);
        this.bio.set(profile.bio ?? '');
        this.website.set(profile.website ?? '');
        this.location.set(profile.location ?? '');
        this.avatarUrl.set(profile.avatarUrl);
        this.tags.set([...(profile.tags ?? [])]);
        this.loading.set(false);
      },
      error: () => {
        this.errorMsg.set('Failed to load profile.');
        this.loading.set(false);
      }
    });
  }

  getInitials(): string {
    const name = this.displayName() || this.username();
    return name.split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2);
  }

  onAvatarFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.avatarUploading.set(true);
    this.errorMsg.set('');
    this.userService.uploadAvatar(file).subscribe({
      next: res => {
        this.avatarUrl.set(res.avatarUrl);
        this.avatarUploading.set(false);
        this.successMsg.set('Avatar updated.');
        setTimeout(() => this.successMsg.set(''), 3000);
      },
      error: err => {
        this.errorMsg.set(err?.error?.message ?? 'Failed to upload avatar.');
        this.avatarUploading.set(false);
      }
    });
    // Reset the file input
    input.value = '';
  }

  removeAvatar(): void {
    this.avatarUrl.set(null);
  }

  addTag(): void {
    const raw = this.newTag().trim().replace(/^#/, '').toLowerCase();
    if (!raw) return;
    if (this.tags().length >= 10) return;
    if (this.tags().includes(raw)) {
      this.newTag.set('');
      return;
    }
    this.tags.update(t => [...t, raw]);
    this.newTag.set('');
  }

  removeTag(tag: string): void {
    this.tags.update(t => t.filter(x => x !== tag));
  }

  onTagKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      event.preventDefault();
      this.addTag();
    }
  }

  save(): void {
    this.saving.set(true);
    this.errorMsg.set('');
    this.successMsg.set('');

    const usernameVal = this.username().trim();
    const displayNameVal = this.displayName().trim();
    if (!displayNameVal) {
      this.errorMsg.set('Display name is required.');
      this.saving.set(false);
      return;
    }

    this.userService.updateProfile({
      username: usernameVal || undefined,
      displayName: displayNameVal,
      bio: this.bio().trim() || undefined,
      website: this.website().trim() || undefined,
      location: this.location().trim() || undefined,
      avatarUrl: this.avatarUrl() ?? undefined,
      tags: this.tags()
    }).subscribe({
      next: updated => {
        this._original = updated;
        this.username.set(updated.username);
        this.avatarUrl.set(updated.avatarUrl);
        this.tags.set([...(updated.tags ?? [])]);
        this.saving.set(false);
        this.saved.set(true);
        this.successMsg.set('Profile saved successfully.');
        setTimeout(() => {
          this.saved.set(false);
          this.successMsg.set('');
        }, 3000);
      },
      error: err => {
        this.errorMsg.set(err?.error?.message ?? 'Failed to save profile.');
        this.saving.set(false);
      }
    });
  }

  cancel(): void {
    if (!this._original) return;
    this.username.set(this._original.username ?? '');
    this.displayName.set(this._original.displayName ?? '');
    this.bio.set(this._original.bio ?? '');
    this.website.set(this._original.website ?? '');
    this.location.set(this._original.location ?? '');
    this.avatarUrl.set(this._original.avatarUrl ?? null);
    this.tags.set([...(this._original.tags ?? [])]);
    this.errorMsg.set('');
    this.successMsg.set('');
  }
}
