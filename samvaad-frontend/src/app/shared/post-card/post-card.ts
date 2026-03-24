import { Component, input, output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Post } from '../../core/models/post.models';
import { PostService } from '../../core/services/post.service';

@Component({
  selector: 'app-post-card',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './post-card.html',
})
export class PostCard {
  post = input.required<Post>();
  postUpdated = output<Post>();
  postDeleted = output<string>();

  private postService = inject(PostService);

  actionInProgress: 'like' | 'bookmark' | 'repost' | 'delete' | null = null;

  toggleLike(): void {
    if (this.actionInProgress) return;
    this.actionInProgress = 'like';
    const p = this.post();
    const action = p.isLiked ? this.postService.unlike(p.id) : this.postService.like(p.id);
    action.subscribe({
      next: updated => { this.postUpdated.emit(updated); this.actionInProgress = null; },
      error: () => { this.actionInProgress = null; }
    });
  }

  toggleBookmark(): void {
    if (this.actionInProgress) return;
    this.actionInProgress = 'bookmark';
    const p = this.post();
    const action = p.isBookmarked ? this.postService.unbookmark(p.id) : this.postService.bookmark(p.id);
    action.subscribe({
      next: updated => { this.postUpdated.emit(updated); this.actionInProgress = null; },
      error: () => { this.actionInProgress = null; }
    });
  }

  toggleRepost(): void {
    if (this.actionInProgress) return;
    this.actionInProgress = 'repost';
    const p = this.post();
    const action = p.isReposted ? this.postService.unrepost(p.id) : this.postService.repost(p.id);
    action.subscribe({
      next: updated => { this.postUpdated.emit(updated); this.actionInProgress = null; },
      error: () => { this.actionInProgress = null; }
    });
  }

  deletePost(): void {
    if (this.actionInProgress) return;
    this.actionInProgress = 'delete';
    const p = this.post();
    this.postService.deletePost(p.id).subscribe({
      next: () => { this.postDeleted.emit(p.id); this.actionInProgress = null; },
      error: () => { this.actionInProgress = null; }
    });
  }

  getInitials(name: string): string {
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  }

  timeAgo(dateStr: string): string {
    const diff = Date.now() - new Date(dateStr).getTime();
    const m = Math.floor(diff / 60000);
    if (m < 1) return 'just now';
    if (m < 60) return `${m}m ago`;
    const h = Math.floor(m / 60);
    if (h < 24) return `${h}h ago`;
    const d = Math.floor(h / 24);
    if (d < 7) return `${d}d ago`;
    return new Date(dateStr).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }

  formatCount(n: number): string {
    if (n >= 1_000_000) return (n / 1_000_000).toFixed(1).replace(/\.0$/, '') + 'M';
    if (n >= 1000) return (n / 1000).toFixed(1).replace(/\.0$/, '') + 'k';
    return n > 0 ? n.toString() : '';
  }
}
