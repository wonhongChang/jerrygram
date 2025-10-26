// User types
export interface User {
  id: string;
  username: string;
  email: string;
  profileImageUrl?: string;
  followerCount: number;
  followingCount: number;
  isFollowing?: boolean;
}

export interface SimpleUser {
  id: string;
  username: string;
  profileImageUrl?: string;
}

// Post types
export interface Post {
  id: string;
  caption: string;
  imageUrl: string;
  createdAt: string;
  likes: number;
  liked: boolean;
  user: SimpleUser;
  score?: number;
}

export interface CreatePostDto {
  caption: string;
  image: File;
  visibility: number;
}

// Comment types
export interface Comment {
  id: string;
  content: string;
  createdAt: string;
  user: SimpleUser;
}

export interface CreateCommentDto {
  content: string;
}

// Auth types
export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  email: string;
  username: string;
  password: string;
}

export interface AuthResponse {
  token: string;
}

// Notification types
export enum NotificationType {
  LIKE = 'LIKE',
  COMMENT = 'COMMENT',
  FOLLOW = 'FOLLOW'
}

export interface Notification {
  id: string;
  message: string;
  type: NotificationType;
  createdAt: string;
  isRead: boolean;
  fromUser: SimpleUser;
  postId?: string;
}

// Search types
export interface SearchResult {
  posts: Post[];
  users: User[];
  hashtags: string[];
}

export interface PopularSearch {
  searchTerm: string;
  count: number;
  rank: number;
  lastSearched: string;
  category: string;
}

// Pagination
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}
