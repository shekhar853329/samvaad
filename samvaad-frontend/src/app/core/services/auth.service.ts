import { Injectable, signal, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { tap, catchError } from 'rxjs/operators';
import { Observable, throwError } from 'rxjs';
import { AuthResponse, LoginRequest, MeResponse, RegisterRequest, UserSummary } from '../models/auth.models';
import { ApiService } from './api.service';

const ACCESS_TOKEN_KEY = 'samvaad_access_token';
const REFRESH_TOKEN_KEY = 'samvaad_refresh_token';
const USER_KEY = 'samvaad_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private api = inject(ApiService);
  private router = inject(Router);

  private _user = signal<UserSummary | null>(this.loadUser());
  private _token = signal<string | null>(localStorage.getItem(ACCESS_TOKEN_KEY));

  readonly user = this._user.asReadonly();
  readonly isLoggedIn = computed(() => this._token() !== null);

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('/auth/login', request).pipe(
      tap(res => this.persist(res))
    );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('/auth/register', request).pipe(
      tap(res => this.persist(res))
    );
  }

  refresh(): Observable<AuthResponse> {
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }
    return this.api.post<AuthResponse>('/auth/refresh', { refreshToken }).pipe(
      tap(res => this.persist(res)),
      catchError(err => {
        this.clear();
        return throwError(() => err);
      })
    );
  }

  logout(): void {
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    this.clear();
    if (refreshToken) {
      this.api.post<void>('/auth/logout', { refreshToken }).subscribe({ error: () => {} });
    }
    this.router.navigate(['/login']);
  }

  getMe(): Observable<MeResponse> {
    return this.api.get<MeResponse>('/auth/me');
  }

  getAccessToken(): string | null {
    return this._token();
  }

  private persist(res: AuthResponse): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, res.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, res.refreshToken);
    localStorage.setItem(USER_KEY, JSON.stringify(res.user));
    this._token.set(res.accessToken);
    this._user.set(res.user);
  }

  private clear(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this._token.set(null);
    this._user.set(null);
  }

  private loadUser(): UserSummary | null {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? JSON.parse(raw) : null;
  }
}
