export type FriendRelation = 'None' | 'RequestSent' | 'RequestReceived' | 'Friends';

export interface UserProfile {
  id: string;
  username: string;
  displayName: string;
  avatarUrl: string | null;
  coverImageUrl: string | null;
  bio: string | null;
  location: string | null;
  website: string | null;
  isVerified: boolean;
  isPrivate: boolean;
  postsCount: number;
  followersCount: number;
  followingCount: number;
  totalViewsCount: number;
  joinedAt: string;
  isFollowing: boolean;
  isOwnProfile: boolean;
  tags: string[];
  friendRelation: FriendRelation;
}

export interface FriendRequestItem {
  id: string;
  senderId: string;
  senderUsername: string;
  senderDisplayName: string;
  senderAvatarUrl: string | null;
  senderIsVerified: boolean;
  createdAt: string;
}

export interface FollowerUser {
  id: string;
  username: string;
  displayName: string;
  avatarUrl: string | null;
  isVerified: boolean;
  isFollowing: boolean;
}

export interface UpdateProfileRequest {
  username?: string;
  displayName?: string;
  bio?: string;
  location?: string;
  website?: string;
  avatarUrl?: string;
  coverImageUrl?: string;
  tags?: string[];
}
