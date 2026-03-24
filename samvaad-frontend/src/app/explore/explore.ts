import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ExploreService } from '../core/services/explore.service';
import { UserService } from '../core/services/user.service';
import { TrendingHashtag } from '../core/models/explore.models';
import { Post } from '../core/models/post.models';
import { FollowerUser } from '../core/models/user.models';
import { PostCard } from '../shared/post-card/post-card';

@Component({
  selector: 'app-explore',
  standalone: true,
  imports: [CommonModule, FormsModule, PostCard],
  templateUrl: './explore.html'
})
export class Explore implements OnInit, OnDestroy {
  private exploreService = inject(ExploreService);
  private userService = inject(UserService);
  private route = inject(ActivatedRoute);

  activeFilter = signal<'all' | 'posts' | 'people' | 'topics' | 'media' | 'live'>('all');

  // Trending data
  trendingPosts = signal<Post[]>([]);
  trendingHashtags = signal<TrendingHashtag[]>([]);
  suggestedPeople = signal<FollowerUser[]>([]);
  trendingLoading = signal(true);

  // Search state
  searchQuery = '';
  searchResultPosts = signal<Post[]>([]);
  searchResultUsers = signal<FollowerUser[]>([]);
  searchLoading = signal(false);
  isSearching = signal(false);

  // Follow state
  followInProgress = new Set<string>();

  private searchSubject = new Subject<string>();
  private subs: Subscription[] = [];

  readonly filters: Array<'all' | 'posts' | 'people' | 'topics' | 'media' | 'live'> =
    ['all', 'posts', 'people', 'topics', 'media', 'live'];

  ngOnInit(): void {
    // Debounced search
    const searchSub = this.searchSubject.pipe(
      debounceTime(350),
      distinctUntilChanged()
    ).subscribe(query => this.executeSearch(query));
    this.subs.push(searchSub);

    // Pre-fill search from query param (e.g. from trending widget)
    const q = this.route.snapshot.queryParamMap.get('q');
    if (q) {
      this.searchQuery = q;
      this.isSearching.set(true);
      this.executeSearch(q);
    }

    this.loadTrendingData();
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
  }

  loadTrendingData(): void {
    this.trendingLoading.set(true);

    this.exploreService.getTrendingPosts().subscribe({
      next: result => { this.trendingPosts.set(result.posts); this.trendingLoading.set(false); },
      error: () => this.trendingLoading.set(false)
    });

    this.exploreService.getTrendingHashtags(5).subscribe({
      next: tags => this.trendingHashtags.set(tags)
    });

    this.userService.getWhoToFollow(4).subscribe({
      next: users => this.suggestedPeople.set(users)
    });
  }

  setFilter(filter: 'all' | 'posts' | 'people' | 'topics' | 'media' | 'live'): void {
    this.activeFilter.set(filter);
    if (this.searchQuery.trim()) {
      this.executeSearch(this.searchQuery.trim());
    }
  }

  onSearchInput(query: string): void {
    const trimmed = query.trim();
    this.isSearching.set(trimmed.length > 0);
    this.searchSubject.next(trimmed);
  }

  clearSearch(): void {
    this.searchQuery = '';
    this.isSearching.set(false);
    this.searchResultPosts.set([]);
    this.searchResultUsers.set([]);
  }

  executeSearch(query: string): void {
    if (!query) {
      this.isSearching.set(false);
      return;
    }
    this.searchLoading.set(true);
    const filter = this.activeFilter();
    const showPosts = filter === 'all' || filter === 'posts' || filter === 'topics' || filter === 'media';
    const showPeople = filter === 'all' || filter === 'people';

    let pending = 0;
    if (showPosts) {
      pending++;
      this.exploreService.searchPosts(query).subscribe({
        next: result => { this.searchResultPosts.set(result.posts); if (--pending === 0) this.searchLoading.set(false); },
        error: () => { if (--pending === 0) this.searchLoading.set(false); }
      });
    } else { this.searchResultPosts.set([]); }

    if (showPeople) {
      pending++;
      this.exploreService.searchUsers(query).subscribe({
        next: result => { this.searchResultUsers.set(result.users); if (--pending === 0) this.searchLoading.set(false); },
        error: () => { if (--pending === 0) this.searchLoading.set(false); }
      });
    } else { this.searchResultUsers.set([]); }

    if (pending === 0) this.searchLoading.set(false);
  }

  searchByHashtag(tag: string): void {
    this.searchQuery = '#' + tag;
    this.isSearching.set(true);
    this.searchLoading.set(true);
    this.exploreService.getPostsByHashtag(tag).subscribe({
      next: result => { this.searchResultPosts.set(result.posts); this.searchResultUsers.set([]); this.searchLoading.set(false); },
      error: () => this.searchLoading.set(false)
    });
  }

  toggleFollow(user: FollowerUser): void {
    if (this.followInProgress.has(user.id)) return;
    this.followInProgress.add(user.id);
    const action = user.isFollowing
      ? this.userService.unfollow(user.username)
      : this.userService.follow(user.username);

    action.subscribe({
      next: () => {
        this.followInProgress.delete(user.id);
        this.suggestedPeople.update(prev => prev.map(u =>
          u.id === user.id ? { ...u, isFollowing: !u.isFollowing } : u
        ));
        this.searchResultUsers.update(prev => prev.map(u =>
          u.id === user.id ? { ...u, isFollowing: !u.isFollowing } : u
        ));
      },
      error: () => this.followInProgress.delete(user.id)
    });
  }

  onPostUpdated(updated: Post): void {
    this.trendingPosts.update(prev => prev.map(p => p.id === updated.id ? updated : p));
    this.searchResultPosts.update(prev => prev.map(p => p.id === updated.id ? updated : p));
  }

  onPostDeleted(id: string): void {
    this.trendingPosts.update(prev => prev.filter(p => p.id !== id));
    this.searchResultPosts.update(prev => prev.filter(p => p.id !== id));
  }

  getInitials(name: string): string {
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  }

  formatCount(n: number): string {
    if (n >= 1_000_000) return (n / 1_000_000).toFixed(1).replace(/\.0$/, '') + 'M';
    if (n >= 1000) return (n / 1000).toFixed(1).replace(/\.0$/, '') + 'k';
    return n > 0 ? n.toString() : '';
  }
}

