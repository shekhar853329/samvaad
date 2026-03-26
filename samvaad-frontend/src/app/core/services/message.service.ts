import { Injectable, inject, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from './api.service';
import { ChatHubService } from './chat-hub.service';
import { Conversation, Message, PagedMessages } from '../models/message.models';

@Injectable({ providedIn: 'root' })
export class MessageService {
  private api = inject(ApiService);
  private chatHub = inject(ChatHubService);

  readonly unreadCount = signal(0);

  constructor() {
    // Increment in real-time whenever a new message arrives for the current user
    this.chatHub.newMessage$.subscribe(({ message }) => {
      if (!message.isMine) {
        this.unreadCount.update(c => c + 1);
      }
    });
  }

  refreshUnreadCount(): void {
    this.api.get<Conversation[]>('/conversations').subscribe({
      next: conversations => {
        const total = conversations.reduce((sum, c) => sum + c.unreadCount, 0);
        this.unreadCount.set(total);
      },
      error: () => {}
    });
  }

  getConversations(): Observable<Conversation[]> {
    return this.api.get<Conversation[]>('/conversations');
  }

  getOrCreateDirect(targetUsername: string): Observable<Conversation> {
    return this.api.post<Conversation>('/conversations/direct', { targetUsername });
  }

  getMessages(conversationId: string, page = 1, pageSize = 30): Observable<PagedMessages> {
    return this.api.get<PagedMessages>(`/conversations/${conversationId}/messages`, { page, pageSize });
  }

  sendMessage(conversationId: string, content: string): Observable<Message> {
    return this.api.post<Message>(`/conversations/${conversationId}/messages`, { content });
  }

  markRead(conversationId: string): Observable<void> {
    return this.api.post<void>(`/conversations/${conversationId}/read`, {}).pipe(
      tap(() => this.refreshUnreadCount())
    );
  }
}
