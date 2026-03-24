import { FollowerUser } from './user.models';
import { PagedPosts } from './post.models';

export interface TrendingHashtag {
  id: string;
  name: string;
  category: string | null;
  postCount: number;
}

export interface PagedUsers {
  users: FollowerUser[];
  hasMore: boolean;
}
