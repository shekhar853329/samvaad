import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { UserService } from '../core/services/user.service';
import { ChatHubService } from '../core/services/chat-hub.service';
import { FloatingChatService } from '../core/services/floating-chat.service';
import { FriendRequestItem } from '../core/models/user.models';

@Component({
  selector: 'app-online-friends',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './online-friends.html'
})
export class OnlineFriends implements OnInit, OnDestroy {
  private userService = inject(UserService);
  private chatHub = inject(ChatHubService);
  readonly floatingChat = inject(FloatingChatService);
  private subs = new Subscription();

  private friends = signal<FriendRequestItem[]>([]);
  private onlineIds = signal<Set<string>>(new Set());

  onlineFriends = computed(() =>
    this.friends().filter(f => this.onlineIds().has(f.senderId))
  );

  ngOnInit(): void {
    console.log('[OnlineFriends] ngOnInit — subscribing');

    // Load friends list
    this.subs.add(
      this.userService.getFriends(1, 100).subscribe({
        next: items => {
          console.log('[OnlineFriends] Loaded friends:', items.length, items.map(f => f.senderUsername));
          this.friends.set(items);
        }
      })
    );

    // Load who is already online right now
    this.subs.add(
      this.userService.getOnlineFriendIds().subscribe({
        next: ids => {
          console.log('[OnlineFriends] Initial online friend IDs:', ids);
          this.onlineIds.set(new Set(ids));
        }
      })
    );

    // Track real-time online events
    this.subs.add(
      this.chatHub.userOnline$.subscribe(userId => {
        console.log('[OnlineFriends] userOnline$ → userId:', userId,
          '| friends loaded:', this.friends().length,
          '| isFriend:', this.friends().some(f => f.senderId === userId));
        const isFriend = this.friends().some(f => f.senderId === userId);
        if (isFriend) {
          this.onlineIds.update(s => new Set([...s, userId]));
          console.log('[OnlineFriends] Added to onlineIds. Current onlineFriends:', this.onlineFriends().length);
        }
      })
    );

    // Track real-time offline events
    this.subs.add(
      this.chatHub.userOffline$.subscribe(userId => {
        console.log('[OnlineFriends] userOffline$ → userId:', userId);
        this.onlineIds.update(s => {
          const next = new Set(s);
          next.delete(userId);
          return next;
        });
        console.log('[OnlineFriends] Removed from onlineIds. Current onlineFriends:', this.onlineFriends().length);
      })
    );
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  openChat(friend: FriendRequestItem, event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    this.floatingChat.openChat(
      friend.senderUsername,
      friend.senderDisplayName,
      friend.senderAvatarUrl
    );
  }

  getInitials(name: string): string {
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  }
}
