import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  CreateSocietyRequest,
  PagedSocieties,
  Society,
  SocietyMember
} from '../models/society.models';
import { PagedPosts, Post, CreatePostRequest } from '../models/post.models';

@Injectable({ providedIn: 'root' })
export class SocietyService {
  private api = inject(ApiService);

  getAll(page = 1, pageSize = 20): Observable<PagedSocieties> {
    return this.api.get<PagedSocieties>('/societies', { page, pageSize });
  }

  getMine(page = 1, pageSize = 20): Observable<PagedSocieties> {
    return this.api.get<PagedSocieties>('/societies/mine', { page, pageSize });
  }

  getById(id: string): Observable<Society> {
    return this.api.get<Society>(`/societies/${id}`);
  }

  create(request: CreateSocietyRequest): Observable<Society> {
    return this.api.post<Society>('/societies', request);
  }

  join(id: string): Observable<Society> {
    return this.api.post<Society>(`/societies/${id}/join`, {});
  }

  leave(id: string): Observable<void> {
    return this.api.delete<void>(`/societies/${id}/leave`);
  }

  getMembers(id: string): Observable<SocietyMember[]> {
    return this.api.get<SocietyMember[]>(`/societies/${id}/members`);
  }

  getFeed(id: string, page = 1, pageSize = 20): Observable<PagedPosts> {
    return this.api.get<PagedPosts>(`/societies/${id}/feed`, { page, pageSize });
  }

  createPost(societyId: string, request: CreatePostRequest): Observable<Post> {
    return this.api.post<Post>(`/societies/${societyId}/posts`, request);
  }
}
