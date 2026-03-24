import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { CreatePostRequest, PagedPosts, Post } from '../models/post.models';

@Injectable({ providedIn: 'root' })
export class PostService {
  private api = inject(ApiService);

  createPost(request: CreatePostRequest): Observable<Post> {
    return this.api.post<Post>('/posts', request);
  }

  getById(id: string): Observable<Post> {
    return this.api.get<Post>(`/posts/${id}`);
  }

  deletePost(id: string): Observable<void> {
    return this.api.delete<void>(`/posts/${id}`);
  }

  like(id: string): Observable<Post> {
    return this.api.post<Post>(`/posts/${id}/like`, {});
  }

  unlike(id: string): Observable<Post> {
    return this.api.delete<Post>(`/posts/${id}/like`);
  }

  bookmark(id: string): Observable<Post> {
    return this.api.post<Post>(`/posts/${id}/bookmark`, {});
  }

  unbookmark(id: string): Observable<Post> {
    return this.api.delete<Post>(`/posts/${id}/bookmark`);
  }

  repost(id: string): Observable<Post> {
    return this.api.post<Post>(`/posts/${id}/repost`, {});
  }

  unrepost(id: string): Observable<Post> {
    return this.api.delete<Post>(`/posts/${id}/repost`);
  }

  getReplies(id: string, page = 1, pageSize = 20): Observable<PagedPosts> {
    return this.api.get<PagedPosts>(`/posts/${id}/replies`, { page, pageSize });
  }

  getFeed(page = 1, pageSize = 20): Observable<PagedPosts> {
    return this.api.get<PagedPosts>('/posts/feed', { page, pageSize });
  }

  getUserPosts(username: string, page = 1, pageSize = 20): Observable<PagedPosts> {
    return this.api.get<PagedPosts>(`/posts/by/${username}`, { page, pageSize });
  }
}
