import { Injectable, inject, signal, NgZone } from '@angular/core';
import { MessageService } from './message.service';
import { ChatHubService } from './chat-hub.service';
import { Message } from '../models/message.models';

export interface FloatingChatUser {
  username: string;
  displayName: string;
  avatarUrl: string | null;
  conversationId: string | null;
}

@Injectable({ providedIn: 'root' })
export class FloatingChatService {
  private msgService = inject(MessageService);
  private chatHub = inject(ChatHubService);
  private ngZone = inject(NgZone);

  readonly activeChat = signal<FloatingChatUser | null>(null);
  readonly messages = signal<Message[]>([]);
  readonly isLoading = signal(false);
  readonly isMinimized = signal(false);

  constructor() {
    this.chatHub.newMessage$.subscribe(({ conversationId, message }) => {
      this.ngZone.run(() => {
        const chat = this.activeChat();
        if (chat?.conversationId !== conversationId) return;
        this.messages.update(list => {
          if (list.some(m => m.id === message.id)) return list;
          return [...list, message];
        });
        this.isMinimized.set(false);
      });
    });
  }

  openChat(username: string, displayName: string, avatarUrl: string | null): void {
    const current = this.activeChat();
    // Same user — just un-minimize
    if (current?.username === username) {
      this.isMinimized.set(false);
      return;
    }

    this.activeChat.set({ username, displayName, avatarUrl, conversationId: null });
    this.messages.set([]);
    this.isMinimized.set(false);
    this.isLoading.set(true);

    this.msgService.getOrCreateDirect(username).subscribe({
      next: conv => {
        // Guard: user may have switched to someone else while request was in-flight
        if (this.activeChat()?.username !== username) return;
        this.activeChat.update(c => c ? { ...c, conversationId: conv.id } : c);
        this.chatHub.joinConversation(conv.id);
        this.loadMessages(conv.id);
      },
      error: () => this.isLoading.set(false),
    });
  }

  closeChat(): void {
    this.activeChat.set(null);
    this.messages.set([]);
  }

  toggleMinimize(): void {
    this.isMinimized.update(v => !v);
  }

  sendMessage(text: string): void {
    const conv = this.activeChat();
    if (!conv?.conversationId || !text.trim()) return;
    this.msgService.sendMessage(conv.conversationId, text.trim()).subscribe({
      next: msg => this.messages.update(list =>
        list.some(m => m.id === msg.id)
          ? list.map(m => m.id === msg.id ? msg : m)  // replace SignalR copy (isMine=false) with REST copy (isMine=true)
          : [...list, msg]
      ),
    });
  }

  private loadMessages(conversationId: string): void {
    this.msgService.getMessages(conversationId, 1, 30).subscribe({
      next: result => {
        this.messages.set(result.messages);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }
}
