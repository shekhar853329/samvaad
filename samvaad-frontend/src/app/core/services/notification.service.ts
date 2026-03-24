import { Injectable, inject, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Notification, PagedNotifications } from '../models/notification.models';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private api = inject(ApiService);

  readonly unreadCount = signal(0);

  refreshUnreadCount(): void {
    this.api.get<{ count: number }>('/notifications/unread-count').subscribe({
      next: res => this.unreadCount.set(res.count),
      error: () => {}
    });
  }

  getNotifications(type?: string, page = 1, pageSize = 20): Observable<PagedNotifications> {
    const params: Record<string, string | number> = { page, pageSize };
    if (type) params['type'] = type;
    return this.api.get<PagedNotifications>('/notifications', params);
  }

  markAsRead(id: string): Observable<void> {
    return this.api.patch<void>(`/notifications/${id}/read`, {});
  }

  markAllAsRead(): Observable<void> {
    return this.api.post<void>('/notifications/read-all', {});
  }
}
