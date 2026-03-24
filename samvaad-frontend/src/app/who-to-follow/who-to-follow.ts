import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { UserService } from '../core/services/user.service';
import { FollowerUser } from '../core/models/user.models';

@Component({
  selector: 'app-who-to-follow',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './who-to-follow.html'
})
export class WhoToFollow implements OnInit {
  private userService = inject(UserService);

  suggestions = signal<FollowerUser[]>([]);
  followingInProgress = signal<Set<string>>(new Set());

  ngOnInit(): void {
    this.userService.getWhoToFollow(5).subscribe({
      next: users => this.suggestions.set(users),
      error: () => {} // silently fail — widget is non-critical
    });
  }

  toggleFollow(user: FollowerUser): void {
    if (this.followingInProgress().has(user.id)) return;

    this.followingInProgress.update(s => new Set(s).add(user.id));

    const action = user.isFollowing
      ? this.userService.unfollow(user.username)
      : this.userService.follow(user.username);

    action.subscribe({
      next: () => {
        this.suggestions.update(list =>
          list.map(u => u.id === user.id ? { ...u, isFollowing: !u.isFollowing } : u)
        );
        this.followingInProgress.update(s => { const ns = new Set(s); ns.delete(user.id); return ns; });
      },
      error: () => {
        this.followingInProgress.update(s => { const ns = new Set(s); ns.delete(user.id); return ns; });
      }
    });
  }

  getInitials(name: string): string {
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  }
}
