import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PostService } from '../core/services/post.service';
import { AuthService } from '../core/services/auth.service';
import { Post } from '../core/models/post.models';
import { PostCard } from '../shared/post-card/post-card';

@Component({
  selector: 'app-feed',
  standalone: true,
  imports: [CommonModule, FormsModule, PostCard],
  templateUrl: './feed.html',
  styleUrl: './feed.css',
})
export class Feed implements OnInit {
  private postService = inject(PostService);
  auth = inject(AuthService);

  posts = signal<Post[]>([]);
  loading = signal(true);
  hasMore = signal(false);
  currentPage = 1;
  readonly pageSize = 20;

  composing = signal(false);
  composeContent = '';
  posting = signal(false);

  ngOnInit(): void {
    this.loadFeed();
  }

  loadFeed(page = 1): void {
    this.loading.set(true);
    this.postService.getFeed(page, this.pageSize).subscribe({
      next: result => {
        if (page === 1) {
          this.posts.set(result.posts);
        } else {
          this.posts.update(prev => [...prev, ...result.posts]);
        }
        this.hasMore.set(result.hasMore);
        this.currentPage = page;
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  loadMore(): void {
    if (!this.hasMore() || this.loading()) return;
    this.loadFeed(this.currentPage + 1);
  }

  submitPost(): void {
    const content = this.composeContent.trim();
    if (!content || this.posting()) return;
    this.posting.set(true);
    this.postService.createPost({ content }).subscribe({
      next: newPost => {
        this.posts.update(prev => [newPost, ...prev]);
        this.composeContent = '';
        this.composing.set(false);
        this.posting.set(false);
      },
      error: () => this.posting.set(false)
    });
  }

  onPostUpdated(updated: Post): void {
    this.posts.update(prev => prev.map(p => p.id === updated.id ? updated : p));
  }

  onPostDeleted(id: string): void {
    this.posts.update(prev => prev.filter(p => p.id !== id));
  }

  getInitials(name: string): string {
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  }

  greetingName(): string {
    const name = this.auth.user()?.displayName;
    if (!name) return 'there';
    return name.split(' ')[0] ?? 'there';
  }
}
