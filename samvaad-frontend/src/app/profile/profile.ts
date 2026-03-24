import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { UserService } from '../core/services/user.service';
import { AuthService } from '../core/services/auth.service';
import { PostService } from '../core/services/post.service';
import { UserProfile } from '../core/models/user.models';
import { Post } from '../core/models/post.models';
import { PostCard } from '../shared/post-card/post-card';

@Component({
  selector: 'app-profile',
  imports: [CommonModule, PostCard],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private userService = inject(UserService);
  private authService = inject(AuthService);
  private postService = inject(PostService);

  profile = signal<UserProfile | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  followLoading = signal(false);
  activeTab = signal<'posts' | 'replies' | 'media' | 'likes' | 'saved'>('posts');

  posts = signal<Post[]>([]);
  postsLoading = signal(false);
  postsHasMore = signal(false);
  private postsPage = 1;
  private currentUsername = '';

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const username = params.get('username');
      if (!username) {
        const currentUser = this.authService.user();
        if (currentUser) {
          this.router.navigate(['/users', currentUser.username]);
        }
        return;
      }
      this.currentUsername = username;
      this.loadProfile(username);
    });
  }

  loadProfile(username: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.posts.set([]);
    this.postsPage = 1;
    this.userService.getProfile(username).subscribe({
      next: p => {
        this.profile.set(p);
        this.loading.set(false);
        this.loadPosts(username, 1);
      },
      error: () => { this.error.set('User not found.'); this.loading.set(false); }
    });
  }

  loadPosts(username: string, page: number): void {
    this.postsLoading.set(true);
    this.postService.getUserPosts(username, page).subscribe({
      next: result => {
        if (page === 1) {
          this.posts.set(result.posts);
        } else {
          this.posts.update(prev => [...prev, ...result.posts]);
        }
        this.postsHasMore.set(result.hasMore);
        this.postsPage = page;
        this.postsLoading.set(false);
      },
      error: () => this.postsLoading.set(false)
    });
  }

  loadMorePosts(): void {
    if (!this.postsHasMore() || this.postsLoading()) return;
    this.loadPosts(this.currentUsername, this.postsPage + 1);
  }

  onPostUpdated(updated: Post): void {
    this.posts.update(prev => prev.map(p => p.id === updated.id ? updated : p));
  }

  onPostDeleted(id: string): void {
    this.posts.update(prev => prev.filter(p => p.id !== id));
  }

  toggleFollow(): void {
    const p = this.profile();
    if (!p || this.followLoading()) return;

    this.followLoading.set(true);
    const action = p.isFollowing
      ? this.userService.unfollow(p.username)
      : this.userService.follow(p.username);

    action.subscribe({
      next: () => {
        this.profile.update(prev => prev ? {
          ...prev,
          isFollowing: !prev.isFollowing,
          followersCount: prev.isFollowing ? prev.followersCount - 1 : prev.followersCount + 1
        } : null);
        this.followLoading.set(false);
      },
      error: () => this.followLoading.set(false)
    });
  }

  getInitials(displayName: string): string {
    return displayName.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  }

  formatCount(n: number): string {
    if (n >= 1_000_000) return (n / 1_000_000).toFixed(1).replace(/\.0$/, '') + 'M';
    if (n >= 1000) return (n / 1000).toFixed(1).replace(/\.0$/, '') + 'k';
    return n.toString();
  }

  readonly tabs: { key: 'posts' | 'replies' | 'media' | 'likes' | 'saved'; label: string }[] = [
    { key: 'posts', label: 'Posts' },
    { key: 'replies', label: 'Replies' },
    { key: 'media', label: 'Media' },
    { key: 'likes', label: 'Likes' },
    { key: 'saved', label: 'Saved' },
  ];

  setTab(tab: 'posts' | 'replies' | 'media' | 'likes' | 'saved'): void {
    this.activeTab.set(tab);
  }
}
