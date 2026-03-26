import { Injectable, inject, signal, OnDestroy, NgZone } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject, Observable } from 'rxjs';
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
  private ngZone = inject(NgZone);
  private hub: signalR.HubConnection | null = null;

  /** Ids of users (that the current user follows or is friends with) who are currently online */
  readonly onlineUserIds = signal<Set<string>>(new Set());

  private readonly userOnlineSubject = new Subject<string>();
  private readonly userOfflineSubject = new Subject<string>();

  /** Emits a userId whenever that user comes online */
  readonly userOnline$: Observable<string> = this.userOnlineSubject.asObservable();
  /** Emits a userId whenever that user goes offline */
  readonly userOffline$: Observable<string> = this.userOfflineSubject.asObservable();

  // Observable stream of incoming messages — for multi-consumer subscriptions
  private readonly newMessageSubject = new Subject<{ conversationId: string; message: Message }>();
  readonly newMessage$ = this.newMessageSubject.asObservable();

  // Callbacks registered by consumers
  private onNewMessage?: (conversationId: string, message: Message) => void;
  private onTyping?: (evt: TypingEvent) => void;
  private onStopTyping?: (evt: { conversationId: string; userId: string }) => void;

  // ── Connection management ────────────────────────────────────────────────

  connect(): void {
    if (this.hub && this.hub.state !== signalR.HubConnectionState.Disconnected) return;

    const hubUrl = environment.apiUrl.replace('/api', '') + '/hubs/chat';
    console.log('[ChatHub] Connecting to', hubUrl);

    this.hub = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => this.auth.getAccessToken() ?? '',
        // Prefer WebSocket so the token is not re-sent on every message.
        // Long Polling re-evaluates accessTokenFactory per request, which
        // would fail after logout() clears the token mid-flight.
        transport: signalR.HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.hub.on('NewMessage', (payload: { conversationId: string; message: Message }) => {
      this.onNewMessage?.(payload.conversationId, payload.message);
      this.newMessageSubject.next(payload);
    });

    this.hub.on('UserTyping', (evt: TypingEvent) => {
      this.onTyping?.(evt);
    });

    this.hub.on('UserStoppedTyping', (evt: { conversationId: string; userId: string }) => {
      this.onStopTyping?.(evt);
    });

    this.hub.on('UserOnline', (userId: string) => {
      console.log('[ChatHub] ← UserOnline received:', userId);
      // SignalR callbacks run outside Angular's NgZone — wrap to trigger change detection
      this.ngZone.run(() => {
        this.onlineUserIds.update(s => new Set([...s, userId]));
        this.userOnlineSubject.next(userId);
        console.log('[ChatHub] userOnline$ emitted for:', userId);
      });
    });

    this.hub.on('UserOffline', (userId: string) => {
      console.log('[ChatHub] ← UserOffline received:', userId);
      this.ngZone.run(() => {
        this.onlineUserIds.update(s => {
          const next = new Set(s);
          next.delete(userId);
          return next;
        });
        this.userOfflineSubject.next(userId);
        console.log('[ChatHub] userOffline$ emitted for:', userId);
      });
    });

    this.hub.onclose(err => {
      console.log('[ChatHub] Connection closed.', err ?? '(clean close)');
    });

    this.hub.onreconnecting(err => {
      console.warn('[ChatHub] Reconnecting...', err);
    });

    this.hub.onreconnected(connId => {
      console.log('[ChatHub] Reconnected. New connId:', connId);
    });

    this.hub.start()
      .then(() => console.log('[ChatHub] Connected ✓', this.hub?.connectionId))
      .catch(err => console.error('[ChatHub] Connection error:', err));
  }

  /**
   * Gracefully logs out from the hub: invokes the server-side Logout method
   * so UserOffline is broadcast immediately, then stops the connection.
   * Returns a Promise so callers can await before clearing credentials.
   */
  disconnectAsync(): Promise<void> {
    console.log('[ChatHub] disconnectAsync() called. Hub state:', this.hub?.state);
    const hub = this.hub;
    this.hub = null; // clear immediately so no re-entrant calls

    if (!hub) return Promise.resolve();

    if (hub.state === signalR.HubConnectionState.Connected) {
      return hub.invoke('Logout')
        .then(() => console.log('[ChatHub] Logout invoked on server ✓'))
        .catch(err => console.warn('[ChatHub] Logout invoke failed (stopping anyway):', err))
        .finally(() => hub.stop().catch(() => {}));
    }

    return hub.stop().catch(() => {});
  }

  /** Fire-and-forget wrapper — used by the reactive effect in App. */
  disconnect(): void {
    this.disconnectAsync().catch(() => {});
  }

  ngOnDestroy(): void {
    this.disconnect();
    this.newMessageSubject.complete();
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
