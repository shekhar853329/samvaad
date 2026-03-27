export interface Society {
  id: string;
  name: string;
  description: string | null;
  avatarUrl: string | null;
  createdByUserId: string;
  createdByUsername: string;
  membersCount: number;
  postsCount: number;
  createdAt: string;
  isMember: boolean;
  isAdmin: boolean;
}

export interface SocietyMember {
  userId: string;
  username: string;
  displayName: string;
  avatarUrl: string | null;
  role: 'Member' | 'Admin';
  joinedAt: string;
}

export interface CreateSocietyRequest {
  name: string;
  description?: string;
}

export interface PagedSocieties {
  societies: Society[];
  hasMore: boolean;
  nextCursor: string | null;
}
