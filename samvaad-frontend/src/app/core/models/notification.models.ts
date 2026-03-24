export interface NotificationActor {
  id: string;
  username: string;
  displayName: string;
  avatarUrl: string | null;
  isVerified: boolean;
}

export interface Notification {
  id: string;
  type: 'Like' | 'Comment' | 'Follow' | 'Repost' | 'Mention' | 'System';
  isRead: boolean;
  createdAt: string;
  actor: NotificationActor | null;
  postId: string | null;
  postSnippet: string | null;
  systemMessage: string | null;
}

export interface PagedNotifications {
  notifications: Notification[];
  hasMore: boolean;
  unreadCount: number;
}
