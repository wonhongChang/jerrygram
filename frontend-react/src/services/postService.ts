import api from './api';
import { Post, PagedResult, CreatePostDto } from '../types';

export const postService = {
  async getPosts(page: number = 1, pageSize: number = 10): Promise<PagedResult<Post>> {
    const response = await api.get<PagedResult<Post>>('/posts', {
      params: { page, pageSize },
    });
    return response.data;
  },

  async getPost(id: string): Promise<Post> {
    const response = await api.get<Post>(`/posts/${id}`);
    return response.data;
  },

  async getFeed(page: number = 1, pageSize: number = 10): Promise<PagedResult<Post>> {
    const response = await api.get<PagedResult<Post>>('/posts/feed', {
      params: { page, pageSize },
    });
    return response.data;
  },

  async getExplorePosts(): Promise<Post[]> {
    const response = await api.get<Post[]>('/explore');
    return response.data;
  },

  async createPost(formData: FormData): Promise<Post> {
    const response = await api.post<Post>('/posts', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  async likePost(id: string): Promise<void> {
    await api.post(`/posts/${id}/like`);
  },

  async unlikePost(id: string): Promise<void> {
    await api.delete(`/posts/${id}/like`);
  },

  async deletePost(id: string): Promise<void> {
    await api.delete(`/posts/${id}`);
  },
};
