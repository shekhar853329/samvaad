import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { TrendingHashtag, PagedUsers } from '../models/explore.models';
import { PagedPosts } from '../models/post.models';

@Injectable({ providedIn: 'root' })
export class ExploreService {
  private api = inject(ApiService);

  getTrendingHashtags(count = 10): Observable<TrendingHashtag[]> {
    return this.api.get<TrendingHashtag[]>('/explore/trending/hashtags', { count });
  }

  getTrendingPosts(page = 1, pageSize = 20): Observable<PagedPosts> {
    return this.api.get<PagedPosts>('/explore/trending/posts', { page, pageSize });
  }

  getPostsByHashtag(tag: string, page = 1, pageSize = 20): Observable<PagedPosts> {
    return this.api.get<PagedPosts>(`/explore/hashtag/${encodeURIComponent(tag)}`, { page, pageSize });
  }

  searchPosts(query: string, page = 1, pageSize = 20): Observable<PagedPosts> {
    return this.api.get<PagedPosts>('/explore/search/posts', { q: query, page, pageSize });
  }

  searchUsers(query: string, page = 1, pageSize = 20): Observable<PagedUsers> {
    return this.api.get<PagedUsers>('/explore/search/users', { q: query, page, pageSize });
  }
}
