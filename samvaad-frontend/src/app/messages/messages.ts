import { Component, signal, inject, OnInit, ElementRef, ViewChild, AfterViewChecked } from '@angular/core';
import { NgClass } from '@angular/common';
import { MessageService } from '../core/services/message.service';
import { Conversation, Message } from '../core/models/message.models';

@Component({
  selector: 'app-messages',
  imports: [NgClass],
  templateUrl: './messages.html'
})
export class Messages implements OnInit, AfterViewChecked {
  private msgService = inject(MessageService);

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

  ngOnInit(): void {
    this.loadConversations();
  }

  ngAfterViewChecked(): void {
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
  }

  loadConversations(): void {
    this.msgService.getConversations().subscribe({
      next: convs => this.conversations.set(convs),
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

  sendMessage(): void {
    const text = this.composeText().trim();
    const conv = this.activeConversation();
    if (!text || !conv) return;

    this.composeText.set('');
    this.msgService.sendMessage(conv.id, text).subscribe({
      next: msg => {
        this.messages.update(list => [...list, msg]);
        this.shouldScrollToBottom = true;
        // Update last message in conversation list
        this.conversations.update(list =>
          list.map(c => c.id === conv.id
            ? { ...c, lastMessageContent: msg.content, lastMessageAt: msg.createdAt }
            : c));
      },
      error: () => this.composeText.set(text)
    });
  }

  onComposeInput(event: Event): void {
    this.composeText.set((event.target as HTMLTextAreaElement).value);
  }

  onComposeKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
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

