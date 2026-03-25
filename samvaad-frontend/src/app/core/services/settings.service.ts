import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  FeedPrefs, NotificationPrefs, PrivacySettings, SessionInfo
} from '../models/settings.models';

@Injectable({ providedIn: 'root' })
export class SettingsService {
  private api = inject(ApiService);

  // Notifications
  getNotificationPrefs(): Observable<NotificationPrefs> {
    return this.api.get<NotificationPrefs>('/settings/notifications');
  }
  saveNotificationPrefs(prefs: NotificationPrefs): Observable<NotificationPrefs> {
    return this.api.put<NotificationPrefs>('/settings/notifications', prefs);
  }

  // Privacy
  getPrivacySettings(): Observable<PrivacySettings> {
    return this.api.get<PrivacySettings>('/settings/privacy');
  }
  savePrivacySettings(settings: PrivacySettings): Observable<PrivacySettings> {
    return this.api.put<PrivacySettings>('/settings/privacy', settings);
  }

  // Feed
  getFeedPrefs(): Observable<FeedPrefs> {
    return this.api.get<FeedPrefs>('/settings/feed');
  }
  saveFeedPrefs(prefs: Omit<FeedPrefs, 'mutedWords'>): Observable<FeedPrefs> {
    return this.api.put<FeedPrefs>('/settings/feed', prefs);
  }
  addMutedWord(word: string): Observable<FeedPrefs> {
    return this.api.post<FeedPrefs>('/settings/feed/muted-words', { word });
  }
  removeMutedWord(word: string): Observable<FeedPrefs> {
    return this.api.delete<FeedPrefs>(`/settings/feed/muted-words/${encodeURIComponent(word)}`);
  }

  // Sessions
  getSessions(): Observable<SessionInfo[]> {
    return this.api.get<SessionInfo[]>('/settings/sessions');
  }
  revokeSession(id: string): Observable<void> {
    return this.api.delete<void>(`/settings/sessions/${id}`);
  }
  revokeAllOtherSessions(): Observable<void> {
    return this.api.delete<void>('/settings/sessions');
  }

  // Account actions
  updateEmail(newEmail: string, currentPassword: string): Observable<void> {
    return this.api.put<void>('/settings/account/email', { newEmail, currentPassword });
  }
  updatePassword(currentPassword: string, newPassword: string): Observable<void> {
    return this.api.put<void>('/settings/account/password', { currentPassword, newPassword });
  }
  deleteAccount(password: string): Observable<void> {
    return this.api.delete<void>('/settings/account', { password });
  }
}
