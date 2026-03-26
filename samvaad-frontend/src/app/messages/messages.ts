import { Component, signal, computed, inject, OnInit, OnDestroy, ElementRef, ViewChild, AfterViewChecked } from '@angular/core';
import { NgClass, CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MessageService } from '../core/services/message.service';
import { UserService } from '../core/services/user.service';
import { ChatHubService } from '../core/services/chat-hub.service';
import { Conversation, Message } from '../core/models/message.models';

@Component({
  selector: 'app-messages',
  imports: [NgClass, CommonModule, RouterLink],
  templateUrl: './messages.html'
})
export class Messages implements OnInit, OnDestroy, AfterViewChecked {
  private msgService = inject(MessageService);
  private userService = inject(UserService);
  readonly hub = inject(ChatHubService);

  @ViewChild('messagesEnd') private messagesEnd!: ElementRef;

  activeTab = signal<'all' | 'groups'>('all');
  conversations = signal<Conversation[]>([]);
  activeConversation = signal<Conversation | null>(null);
  messages = signal<Message[]>([]);
  hasMoreMessages = signal(false);
  loadingMessages = signal(false);
  private messagePage = 1;
  private shouldScrollToBottom = false;

  composeText = signal('');

  // Typing indicators: map of conversationId → displayName of the person typing
  typingUsers = signal<Map<string, string>>(new Map());
  private typingTimers = new Map<string, ReturnType<typeof setTimeout>>();

  // Online users panel: derived reactively from conversations + hub online set
  readonly onlineFollowingProfiles = computed(() =>
    this.conversations()
      .filter(c => !c.isGroup && c.otherParticipants[0] && this.hub.onlineUserIds().has(c.otherParticipants[0].userId))
      .map(c => ({
        id: c.otherParticipants[0].userId,
        username: c.otherParticipants[0].username,
        displayName: c.otherParticipants[0].displayName,
        avatarUrl: c.otherParticipants[0].avatarUrl,
      }))
  );

  ngOnInit(): void {
    // Connect to SignalR hub
    this.hub.connect();

    // Register real-time callbacks
    this.hub.onMessage((convId, msg) => {
      const active = this.activeConversation();
      if (active?.id === convId) {
        // Deduplicate by ID: backend broadcasts isMine=false to everyone including
        // the sender, so REST-added messages would otherwise appear twice.
        this.messages.update(list => {
          if (list.some(m => m.id === msg.id)) return list;
          return [...list, msg];
        });
        this.shouldScrollToBottom = true;
        // Mark as read since window is open
        this.msgService.markRead(convId).subscribe();
      }
      // Update conversation preview
      this.conversations.update(list =>
        list.map(c => c.id === convId
          ? { ...c, lastMessageContent: msg.content, lastMessageAt: msg.createdAt,
              unreadCount: active?.id === convId ? 0 : c.unreadCount + 1 }
          : c));
    });

    this.hub.onUserTyping(evt => {
      this.typingUsers.update(m => {
        const next = new Map(m);
        next.set(evt.conversationId, evt.displayName);
        return next;
      });
      // Auto-clear after 3 s if no stop event
      const key = evt.conversationId + ':' + evt.userId;
      if (this.typingTimers.has(key)) clearTimeout(this.typingTimers.get(key));
      this.typingTimers.set(key, setTimeout(() => this.clearTyping(evt.conversationId, key), 3000));
    });

    this.hub.onUserStoppedTyping(evt => {
      const key = evt.conversationId + ':' + evt.userId;
      this.clearTyping(evt.conversationId, key);
    });

    this.loadConversations();
    this.loadOnlineFollowing();
  }

  ngOnDestroy(): void {
    this.typingTimers.forEach(t => clearTimeout(t));
    // Keep hub connected (app-level) — only disconnect on logout
  }

  ngAfterViewChecked(): void {
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
  }

  private clearTyping(conversationId: string, key: string): void {
    this.typingUsers.update(m => {
      const next = new Map(m);
      next.delete(conversationId);
      return next;
    });
    this.typingTimers.delete(key);
  }

  loadConversations(): void {
    this.msgService.getConversations().subscribe({
      next: convs => this.conversations.set(convs),
      error: () => {}
    });
  }

  loadOnlineFollowing(): void {
    this.userService.getOnlineFollowing().subscribe({
      next: (onlineIds: string[]) => {
        // Seed the hub's onlineUserIds signal with the REST snapshot.
        // The computed() will automatically re-derive the panel.
        this.hub.onlineUserIds.set(new Set(onlineIds));
      },
      error: () => {}
    });
  }

  filteredConversations(): Conversation[] {
    const tab = this.activeTab();
    if (tab === 'groups') return this.conversations().filter(c => c.isGroup);
    return this.conversations();
  }

  selectConversation(conv: Conversation): void {
    this.activeConversation.set(conv);
    this.messages.set([]);
    this.messagePage = 1;
    this.loadMessages(false);
    this.hub.joinConversation(conv.id);
    this.msgService.markRead(conv.id).subscribe(() => {
      this.conversations.update(list =>
        list.map(c => c.id === conv.id ? { ...c, unreadCount: 0 } : c));
    });
  }

  backToList(): void {
    this.activeConversation.set(null);
  }

  loadMoreMessages(): void {
    this.messagePage++;
    this.loadMessages(true);
  }

  private typingDebounce: ReturnType<typeof setTimeout> | null = null;

  sendMessage(): void {
    const text = this.composeText().trim();
    const conv = this.activeConversation();
    if (!text || !conv) return;

    this.composeText.set('');
    this.hub.sendStopTyping(conv.id);

    this.msgService.sendMessage(conv.id, text).subscribe({
      next: msg => {
        this.messages.update(list =>
          list.some(m => m.id === msg.id)
            ? list.map(m => m.id === msg.id ? msg : m)
            : [...list, msg]
        );
        this.shouldScrollToBottom = true;
        this.conversations.update(list =>
          list.map(c => c.id === conv.id
            ? { ...c, lastMessageContent: msg.content, lastMessageAt: msg.createdAt }
            : c));
      },
      error: () => this.composeText.set(text)
    });
  }

  onComposeInput(event: Event): void {
    const value = (event.target as HTMLTextAreaElement).value;
    this.composeText.set(value);

    const conv = this.activeConversation();
    if (!conv) return;

    // Send typing indicator with debounce
    this.hub.sendTyping(conv.id);
    if (this.typingDebounce) clearTimeout(this.typingDebounce);
    this.typingDebounce = setTimeout(() => this.hub.sendStopTyping(conv.id), 2000);
  }

  onComposeKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  isOtherUserOnline(conv: Conversation): boolean {
    if (conv.isGroup) return false;
    const otherId = conv.otherParticipants[0]?.userId;
    return otherId ? this.hub.isUserOnline(otherId) : false;
  }

  convName(conv: Conversation): string {
    if (conv.isGroup) return conv.name ?? 'Group';
    return conv.otherParticipants[0]?.displayName ?? 'Unknown';
  }

  convLastLine(conv: Conversation): string {
    if (!conv.lastMessageContent) return '';
    if (conv.isGroup && conv.lastMessageSenderName)
      return `${conv.lastMessageSenderName.split(' ')[0]}: ${conv.lastMessageContent}`;
    return conv.lastMessageContent;
  }

  initials(name: string): string {
    return name.split(' ').map(w => w[0]).slice(0, 2).join('').toUpperCase();
  }

  timeAgo(dateStr: string | null): string {
    if (!dateStr) return '';
    const diff = Math.floor((Date.now() - new Date(dateStr).getTime()) / 1000);
    if (diff < 60) return `${diff}s`;
    if (diff < 3600) return `${Math.floor(diff / 60)}m`;
    if (diff < 86400) return `${Math.floor(diff / 3600)}h`;
    if (diff < 172800) return 'Yesterday';
    return new Date(dateStr).toLocaleDateString('en-US', { weekday: 'short' });
  }

  formatTime(dateStr: string): string {
    return new Date(dateStr).toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit' });
  }

  private loadMessages(prepend: boolean): void {
    const conv = this.activeConversation();
    if (!conv || this.loadingMessages()) return;
    this.loadingMessages.set(true);
    this.msgService.getMessages(conv.id, this.messagePage).subscribe({
      next: result => {
        if (prepend) this.messages.update(list => [...result.messages, ...list]);
        else {
          this.messages.set(result.messages);
          this.shouldScrollToBottom = true;
        }
        this.hasMoreMessages.set(result.hasMore);
        this.loadingMessages.set(false);
      },
      error: () => this.loadingMessages.set(false)
    });
  }

  private scrollToBottom(): void {
    try { this.messagesEnd?.nativeElement?.scrollIntoView({ behavior: 'smooth' }); }
    catch { /* ignore */ }
  }
}

