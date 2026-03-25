import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { UserService } from '../core/services/user.service';
import { FriendRequestItem } from '../core/models/user.models';

@Component({
  selector: 'app-friends-page',
  imports: [CommonModule, RouterLink],
  templateUrl: './friends-page.html',
})
export class FriendsPage implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private userService = inject(UserService);

  username = signal('');
  friends = signal<FriendRequestItem[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  hasMore = signal(false);
  private page = 1;
  private readonly pageSize = 20;

  goBack(): void {
    this.router.navigate(['/users', this.username()]);
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const u = params.get('username');
      if (!u) { this.router.navigate(['/']); return; }
      this.username.set(u);
      this.loadFriends(1);
    });
  }

  loadFriends(page: number): void {
    this.loading.set(true);
    this.userService.getFriendsByUsername(this.username(), page, this.pageSize).subscribe({
      next: items => {
        if (page === 1) this.friends.set(items);
        else this.friends.update(prev => [...prev, ...items]);
        this.hasMore.set(items.length === this.pageSize);
        this.page = page;
        this.loading.set(false);
      },
      error: () => { this.error.set('Could not load friends.'); this.loading.set(false); }
    });
  }

  loadMore(): void {
    if (!this.hasMore() || this.loading()) return;
    this.loadFriends(this.page + 1);
  }

  getInitials(name: string): string {
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  }
}
