export interface ConversationParticipant {
  userId: string;
  username: string;
  displayName: string;
  avatarUrl: string | null;
}

export interface Conversation {
  id: string;
  isGroup: boolean;
  name: string | null;
  otherParticipants: ConversationParticipant[];
  lastMessageContent: string | null;
  lastMessageSenderName: string | null;
  lastMessageAt: string | null;
  unreadCount: number;
}

export interface Message {
  id: string;
  senderId: string;
  senderDisplayName: string;
  senderAvatarUrl: string | null;
  content: string;
  createdAt: string;
  isMine: boolean;
}

export interface PagedMessages {
  messages: Message[];
  hasMore: boolean;
}
