import { Injectable, inject, signal, OnDestroy } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { Message, Conversation } from '../models/message.models';

export interface TypingEvent {
  conversationId: string;
  userId: string;
  displayName: string;
}

@Injectable({ providedIn: 'root' })
export class ChatHubService implements OnDestroy {
  private auth = inject(AuthService);
  private hub: signalR.HubConnection | null = null;

  /** Ids of users (that the current user follows) who are currently online */
  readonly onlineUserIds = signal<Set<string>>(new Set());

  // Callbacks registered by consumers
  private onNewMessage?: (conversationId: string, message: Message) => void;
  private onTyping?: (evt: TypingEvent) => void;
  private onStopTyping?: (evt: { conversationId: string; userId: string }) => void;

  // ── Connection management ────────────────────────────────────────────────

  connect(): void {
    if (this.hub && this.hub.state !== signalR.HubConnectionState.Disconnected) return;

    const hubUrl = environment.apiUrl.replace('/api', '') + '/hubs/chat';

    this.hub = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => this.auth.getAccessToken() ?? '',
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.hub.on('NewMessage', (payload: { conversationId: string; message: Message }) => {
      this.onNewMessage?.(payload.conversationId, payload.message);
    });

    this.hub.on('UserTyping', (evt: TypingEvent) => {
      this.onTyping?.(evt);
    });

    this.hub.on('UserStoppedTyping', (evt: { conversationId: string; userId: string }) => {
      this.onStopTyping?.(evt);
    });

    this.hub.on('UserOnline', (userId: string) => {
      this.onlineUserIds.update(s => new Set([...s, userId]));
    });

    this.hub.on('UserOffline', (userId: string) => {
      this.onlineUserIds.update(s => {
        const next = new Set(s);
        next.delete(userId);
        return next;
      });
    });

    this.hub.start().catch(err => console.error('SignalR connect error', err));
  }

  disconnect(): void {
    this.hub?.stop().catch(() => {});
    this.hub = null;
  }

  ngOnDestroy(): void {
    this.disconnect();
  }

  // ── Server methods ───────────────────────────────────────────────────────

  sendTyping(conversationId: string): void {
    this.hub?.invoke('Typing', conversationId).catch(() => {});
  }

  sendStopTyping(conversationId: string): void {
    this.hub?.invoke('StopTyping', conversationId).catch(() => {});
  }

  joinConversation(conversationId: string): void {
    this.hub?.invoke('JoinConversation', conversationId).catch(() => {});
  }

  // ── Callback registration ────────────────────────────────────────────────

  onMessage(cb: (conversationId: string, message: Message) => void): void {
    this.onNewMessage = cb;
  }

  onUserTyping(cb: (evt: TypingEvent) => void): void {
    this.onTyping = cb;
  }

  onUserStoppedTyping(cb: (evt: { conversationId: string; userId: string }) => void): void {
    this.onStopTyping = cb;
  }

  isUserOnline(userId: string): boolean {
    return this.onlineUserIds().has(userId);
  }
}
