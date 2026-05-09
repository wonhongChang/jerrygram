import api, { getPaginationParams } from './api';
import { Post, PagedResult } from '../types';
import { normalizePagedResult, normalizePost } from '../utils/apiData';

export const postService = {
  async getPosts(page: number = 1, pageSize: number = 10): Promise<PagedResult<Post>> {
    const response = await api.get('/posts', {
      params: getPaginationParams(page, pageSize),
    });
    return normalizePagedResult(response.data, normalizePost);
  },

  async getPost(id: string): Promise<Post> {
    const response = await api.get(`/posts/${id}`);
    return normalizePost(response.data);
  },

  async getFeed(page: number = 1, pageSize: number = 10): Promise<PagedResult<Post>> {
    const response = await api.get('/posts/feed', {
      params: getPaginationParams(page, pageSize),
    });
    return normalizePagedResult(response.data, normalizePost);
  },

  async getSavedPosts(page: number = 1, pageSize: number = 30): Promise<PagedResult<Post>> {
    const response = await api.get('/posts/saved', {
      params: getPaginationParams(page, pageSize),
    });
    return normalizePagedResult(response.data, normalizePost);
  },

  async getExplorePosts(): Promise<Post[]> {
    const response = await api.get('/explore');
    return Array.isArray(response.data)
      ? response.data.map(normalizePost)
      : normalizePagedResult(response.data, normalizePost).items;
  },

  async createPost(formData: FormData): Promise<Post> {
    const response = await api.post('/posts', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return normalizePost(response.data);
  },

  async likePost(id: string): Promise<void> {
    await api.post(`/posts/${id}/like`);
  },

  async unlikePost(id: string): Promise<void> {
    try {
      await api.delete(`/posts/${id}/like`);
    } catch (error: any) {
      if (error?.response?.status !== 404 && error?.response?.status !== 405) throw error;
      await api.post(`/posts/${id}/like`);
    }
  },

  async savePost(id: string): Promise<void> {
    await api.post(`/posts/${id}/save`);
  },

  async unsavePost(id: string): Promise<void> {
    await api.delete(`/posts/${id}/save`);
  },

  async deletePost(id: string): Promise<void> {
    await api.delete(`/posts/${id}`);
  },
};
