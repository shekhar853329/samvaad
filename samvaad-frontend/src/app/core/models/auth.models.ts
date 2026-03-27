export interface MeResponse {
  userId: string;
  username: string;
  email: string;
}

export interface LoginRequest {
  identifier: string;
  password: string;
}

export interface GoogleLoginRequest {
  credential: string;
}

export interface RegisterRequest {
  username: string;
  displayName: string;
  email: string;
  password: string;
}

export interface UserSummary {
  id: string;
  username: string;
  displayName: string;
  email: string;
  avatarUrl: string | null;
  isVerified: boolean;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserSummary;
}
