import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Conversation, Message, PagedMessages } from '../models/message.models';

@Injectable({ providedIn: 'root' })
export class MessageService {
  private api = inject(ApiService);

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
    return this.api.post<void>(`/conversations/${conversationId}/read`, {});
  }
}
