import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { FollowerUser, UpdateProfileRequest, UserProfile } from '../models/user.models';

@Injectable({ providedIn: 'root' })
export class UserService {
  private api = inject(ApiService);

  getMyProfile(): Observable<UserProfile> {
    return this.api.get<UserProfile>('/users/me');
  }

  getProfile(username: string): Observable<UserProfile> {
    return this.api.get<UserProfile>(`/users/${username}`);
  }

  updateProfile(request: UpdateProfileRequest): Observable<UserProfile> {
    return this.api.put<UserProfile>('/users/me', request);
  }

  uploadAvatar(file: File): Observable<{ avatarUrl: string }> {
    const form = new FormData();
    form.append('file', file);
    return this.api.postForm<{ avatarUrl: string }>('/users/me/avatar', form);
  }

  follow(username: string): Observable<void> {
    return this.api.post<void>(`/users/${username}/follow`, {});
  }

  unfollow(username: string): Observable<void> {
    return this.api.delete<void>(`/users/${username}/follow`);
  }

  getFollowers(username: string, page = 1, pageSize = 20): Observable<FollowerUser[]> {
    return this.api.get<FollowerUser[]>(`/users/${username}/followers`, { page, pageSize });
  }

  getFollowing(username: string, page = 1, pageSize = 20): Observable<FollowerUser[]> {
    return this.api.get<FollowerUser[]>(`/users/${username}/following`, { page, pageSize });
  }

  getWhoToFollow(count = 5): Observable<FollowerUser[]> {
    return this.api.get<FollowerUser[]>('/users/who-to-follow', { count });
  }
}
