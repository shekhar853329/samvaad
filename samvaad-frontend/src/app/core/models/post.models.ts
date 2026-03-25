export interface PostAuthor {
  id: string;
  username: string;
  displayName: string;
  avatarUrl: string | null;
  isVerified: boolean;
}

export interface PostMedia {
  id: string;
  url: string;
  mediaType: 'Image' | 'Video' | 'Gif' | 'File';
  width: number | null;
  height: number | null;
  displayOrder: number;
}

export interface Post {
  id: string;
  author: PostAuthor;
  content: string;
  createdAt: string;
  likesCount: number;
  commentsCount: number;
  repostsCount: number;
  viewsCount: number;
  isLiked: boolean;
  isBookmarked: boolean;
  isReposted: boolean;
  isOwnPost: boolean;
  isFollowingAuthor: boolean;
  media: PostMedia[];
  hashtags: string[];
  parentPostId: string | null;
  repostOfId: string | null;
}

export interface PagedPosts {
  posts: Post[];
  hasMore: boolean;
  nextCursor: string | null;
}

export interface CreatePostRequest {
  content: string;
  parentPostId?: string;
  repostOfId?: string;
}
