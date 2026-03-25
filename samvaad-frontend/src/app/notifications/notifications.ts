import { Component, signal, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NotificationService } from '../core/services/notification.service';
import { Notification } from '../core/models/notification.models';

const TABS = ['all', 'mention', 'follow', 'like', 'repost', 'system'] as const;
type Tab = typeof TABS[number];

const TAB_LABELS: Record<Tab, string> = {
  all: 'All', mention: 'Mentions', follow: 'Follows',
  like: 'Likes', repost: 'Reposts', system: 'System'
};

const TAB_TYPES: Record<Tab, string | undefined> = {
  all: undefined, mention: 'Mention', follow: 'Follow',
  like: 'Like', repost: 'Repost', system: 'System'
};

@Component({
  selector: 'app-notifications',
  imports: [],
  templateUrl: './notifications.html',
  styleUrl: './notifications.css',
})
export class Notifications implements OnInit {
  private notifService = inject(NotificationService);
  private router = inject(Router);

  readonly tabs = TABS;
  readonly tabLabels = TAB_LABELS;
  activeTab = signal<Tab>('all');
  items = signal<Notification[]>([]);
  hasMore = signal(false);
  loading = signal(false);
  private page = signal(1);

  ngOnInit(): void {
    this.load(true);
  }

  setTab(tab: Tab): void {
    if (this.activeTab() === tab) return;
    this.activeTab.set(tab);
    this.page.set(1);
    this.items.set([]);
    this.load(true);
  }

  loadMore(): void {
    this.page.update(p => p + 1);
    this.load(false);
  }

  handleClick(n: Notification): void {
    if (!n.isRead) {
      this.notifService.markAsRead(n.id).subscribe(() => {
        this.items.update(list => list.map(x => x.id === n.id ? { ...x, isRead: true } : x));
        this.notifService.unreadCount.update(c => Math.max(0, c - 1));
      });
    }
    if (n.actor) {
      this.router.navigate(['/users', n.actor.username]);
    }
  }

  markAllRead(): void {
    this.notifService.markAllAsRead().subscribe(() => {
      this.items.update(list => list.map(n => ({ ...n, isRead: true })));
      this.notifService.unreadCount.set(0);
    });
  }

  iconFor(type: string): string {
    return ({ Like: '❤', Comment: '💬', Follow: '👤', Repost: '↗', Mention: '@', System: '🔔' } as Record<string, string>)[type] ?? '🔔';
  }

  bgFor(type: string): string {
    return ({ Like: '#FAECE7', Comment: '#E6F1FB', Follow: '#EEEDFE', Repost: '#E1F5EE', Mention: '#EAF3DE' } as Record<string, string>)[type] ?? 'transparent';
  }

  textFor(n: Notification): string {
    const name = n.actor?.displayName ?? 'Someone';
    switch (n.type) {
      case 'Like': return `${name} liked your post`;
      case 'Comment': return `${name} replied to your post`;
      case 'Follow': return `${name} followed you`;
      case 'Repost': return `${name} reposted your post`;
      case 'Mention': return `${name} mentioned you in a post`;
      case 'System': return n.systemMessage ?? 'System notification';
      default: return 'New notification';
    }
  }

  timeAgo(dateStr: string): string {
    const diff = Math.floor((Date.now() - new Date(dateStr).getTime()) / 1000);
    if (diff < 60) return `${diff}s`;
    if (diff < 3600) return `${Math.floor(diff / 60)}m`;
    if (diff < 86400) return `${Math.floor(diff / 3600)}h`;
    return `${Math.floor(diff / 86400)}d`;
  }

  initials(name: string): string {
    return name.split(' ').map(w => w[0]).slice(0, 2).join('').toUpperCase();
  }

  private load(reset: boolean): void {
    if (this.loading()) return;
    this.loading.set(true);
    this.notifService.getNotifications(TAB_TYPES[this.activeTab()], this.page(), 20).subscribe({
      next: result => {
        if (reset) this.items.set(result.notifications);
        else this.items.update(list => [...list, ...result.notifications]);
        this.hasMore.set(result.hasMore);
        this.notifService.unreadCount.set(result.unreadCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}

