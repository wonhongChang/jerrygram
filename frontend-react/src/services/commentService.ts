import api from './api';
import { Comment, CreateCommentDto } from '../types';

export const commentService = {
  async getComments(postId: string): Promise<Comment[]> {
    const response = await api.get<Comment[]>(`/posts/${postId}/comments`);
    return response.data;
  },

  async createComment(postId: string, data: CreateCommentDto): Promise<Comment> {
    const response = await api.post<Comment>(`/posts/${postId}/comments`, data);
    return response.data;
  },

  async deleteComment(commentId: string): Promise<void> {
    await api.delete(`/comments/${commentId}`);
  },
};
