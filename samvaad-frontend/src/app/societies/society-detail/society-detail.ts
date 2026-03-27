import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { SocietyService } from '../../core/services/society.service';
import { AuthService } from '../../core/services/auth.service';
import { Society, SocietyMember } from '../../core/models/society.models';
import { Post } from '../../core/models/post.models';
import { PostCard } from '../../shared/post-card/post-card';

@Component({
  selector: 'app-society-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, PostCard],
  templateUrl: './society-detail.html',
  styleUrl: './society-detail.css',
})
export class SocietyDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private societyService = inject(SocietyService);
  auth = inject(AuthService);

  society = signal<Society | null>(null);
  members = signal<SocietyMember[]>([]);
  posts = signal<Post[]>([]);
  loading = signal(true);
  loadingFeed = signal(true);
  hasMore = signal(false);
  currentPage = 1;
  readonly pageSize = 20;

  composing = signal(false);
  composeContent = '';
  posting = signal(false);

  activePanel = signal<'feed' | 'members'>('feed');
  joiningLeaving = signal(false);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.loadSociety(id);
  }

  private get societyId(): string {
    return this.route.snapshot.paramMap.get('id')!;
  }

  loadSociety(id: string): void {
    this.loading.set(true);
    this.societyService.getById(id).subscribe({
      next: society => {
        this.society.set(society);
        this.loading.set(false);
        if (society.isMember) {
          this.loadFeed();
          this.loadMembers();
        }
      },
      error: () => {
        this.loading.set(false);
        this.router.navigate(['/societies']);
      }
    });
  }

  loadFeed(page = 1): void {
    if (!this.society()?.isMember) return;
    this.loadingFeed.set(true);
    this.societyService.getFeed(this.societyId, page, this.pageSize).subscribe({
      next: result => {
        if (page === 1) {
          this.posts.set(result.posts);
        } else {
          this.posts.update(prev => [...prev, ...result.posts]);
        }
        this.hasMore.set(result.hasMore);
        this.currentPage = page;
        this.loadingFeed.set(false);
      },
      error: () => this.loadingFeed.set(false)
    });
  }

  loadMembers(): void {
    this.societyService.getMembers(this.societyId).subscribe({
      next: members => this.members.set(members)
    });
  }

  loadMore(): void {
    if (!this.hasMore() || this.loadingFeed()) return;
    this.loadFeed(this.currentPage + 1);
  }

  join(): void {
    if (this.joiningLeaving()) return;
    this.joiningLeaving.set(true);
    this.societyService.join(this.societyId).subscribe({
      next: updated => {
        this.society.set(updated);
        this.joiningLeaving.set(false);
        this.loadFeed();
        this.loadMembers();
      },
      error: () => this.joiningLeaving.set(false)
    });
  }

  leave(): void {
    if (this.joiningLeaving()) return;
    this.joiningLeaving.set(true);
    this.societyService.leave(this.societyId).subscribe({
      next: () => {
        this.society.update(s => s ? { ...s, isMember: false, isAdmin: false, membersCount: s.membersCount - 1 } : s);
        this.joiningLeaving.set(false);
        this.posts.set([]);
        this.members.set([]);
      },
      error: (err) => {
        alert(err?.error?.message ?? 'Could not leave society.');
        this.joiningLeaving.set(false);
      }
    });
  }

  submitPost(): void {
    const content = this.composeContent.trim();
    if (!content || this.posting()) return;
    this.posting.set(true);
    this.societyService.createPost(this.societyId, { content }).subscribe({
      next: newPost => {
        this.posts.update(prev => [newPost, ...prev]);
        this.society.update(s => s ? { ...s, postsCount: s.postsCount + 1 } : s);
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
    this.society.update(s => s ? { ...s, postsCount: Math.max(0, s.postsCount - 1) } : s);
  }

  getInitials(name: string): string {
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  }

  goBack(): void {
    this.router.navigate(['/societies']);
  }
}
