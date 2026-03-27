import { Component, inject, OnInit, signal } from '@angular/core';
import { PostService } from '../core/services/post.service';
import { Post } from '../core/models/post.models';
import { StoriesBar } from './stories-bar/stories-bar';
import { ComposeBox } from './compose-box/compose-box';
import { PostsList } from './posts-list/posts-list';

@Component({
  selector: 'app-feed',
  standalone: true,
  imports: [StoriesBar, ComposeBox, PostsList],
  templateUrl: './feed.html',
  styleUrl: './feed.css',
})
export class Feed implements OnInit {
  private postService = inject(PostService);

  posts = signal<Post[]>([]);
  loading = signal(true);
  hasMore = signal(false);
  currentPage = 1;
  readonly pageSize = 20;

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
      error: () => this.loading.set(false),
    });
  }

  loadMore(): void {
    if (!this.hasMore() || this.loading()) return;
    this.loadFeed(this.currentPage + 1);
  }

  onPostCreated(newPost: Post): void {
    this.posts.update(prev => [newPost, ...prev]);
  }

  onPostUpdated(updated: Post): void {
    this.posts.update(prev => prev.map(p => p.id === updated.id ? updated : p));
  }

  onPostDeleted(id: string): void {
    this.posts.update(prev => prev.filter(p => p.id !== id));
  }
}
