import api from './api';
import { Comment, CreateCommentDto } from '../types';
import { normalizeComment, normalizePagedResult } from '../utils/apiData';

export const commentService = {
  async getComments(postId: string): Promise<Comment[]> {
    try {
      const response = await api.get(`/posts/${postId}/comments`);
      return Array.isArray(response.data)
        ? response.data.map(normalizeComment)
        : normalizePagedResult(response.data, normalizeComment).items;
    } catch (error: any) {
      if (error?.response?.status !== 404) throw error;
      const response = await api.get(`/comments/post/${postId}`);
      return normalizePagedResult(response.data, normalizeComment).items;
    }
  },

  async createComment(postId: string, data: CreateCommentDto): Promise<Comment> {
    try {
      const response = await api.post(`/posts/${postId}/comments`, data);
      return normalizeComment(response.data);
    } catch (error: any) {
      if (error?.response?.status !== 404 && error?.response?.status !== 405) throw error;
      const response = await api.post('/comments', { postId, content: data.content });
      return normalizeComment(response.data);
    }
  },

  async deleteComment(commentId: string): Promise<void> {
    await api.delete(`/comments/${commentId}`);
  },
};
