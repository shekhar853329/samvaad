import { Component, OnInit, signal, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SettingsService } from '../../core/services/settings.service';
import { FeedPrefs } from '../../core/models/settings.models';

@Component({
  selector: 'app-settings-feed',
  imports: [FormsModule],
  templateUrl: './feed.html'
})
export class SettingsFeed implements OnInit {
  private settingsService = inject(SettingsService);
  prefs = signal<FeedPrefs | null>(null);
  loading = signal(true);
  saving = signal(false);
  saved = signal(false);
  success = signal('');
  error = signal('');
  newWord = signal('');
  addingWord = signal(false);
  removingWord = signal<string | null>(null);

  ngOnInit(): void {
    this.settingsService.getFeedPrefs().subscribe({
      next: p => { this.prefs.set(p); this.loading.set(false); },
      error: () => { this.error.set('Could not load preferences.'); this.loading.set(false); }
    });
  }

  set<K extends keyof Omit<FeedPrefs, 'mutedWords'>>(key: K, value: FeedPrefs[K]): void {
    const p = this.prefs();
    if (p) this.prefs.set({ ...p, [key]: value });
  }

  save(): void {
    const p = this.prefs();
    if (!p) return;
    this.saving.set(true);
    this.error.set('');
    const { mutedWords, ...rest } = p;
    this.settingsService.saveFeedPrefs(rest).subscribe({
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

  addWord(): void {
    const word = this.newWord().trim().toLowerCase();
    if (!word) return;
    const p = this.prefs();
    if (p?.mutedWords.includes(word)) return;
    this.addingWord.set(true);
    this.settingsService.addMutedWord(word).subscribe({
      next: updated => { this.prefs.set(updated); this.newWord.set(''); this.addingWord.set(false); },
      error: () => { this.addingWord.set(false); }
    });
  }

  removeWord(word: string): void {
    this.removingWord.set(word);
    this.settingsService.removeMutedWord(word).subscribe({
      next: updated => { this.prefs.set(updated); this.removingWord.set(null); },
      error: () => { this.removingWord.set(null); }
    });
  }
}
