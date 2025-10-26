import api from './api';
import { User, SimpleUser } from '../types';

export const userService = {
  async getCurrentUser(): Promise<User> {
    const response = await api.get<User>('/users/me');
    return response.data;
  },

  async getUserProfile(username: string): Promise<User> {
    const response = await api.get<User>(`/users/${username}`);
    return response.data;
  },

  async followUser(userId: string): Promise<void> {
    await api.post(`/users/${userId}/follow`);
  },

  async unfollowUser(userId: string): Promise<void> {
    await api.delete(`/users/${userId}/unfollow`);
  },

  async getFollowers(userId: string): Promise<SimpleUser[]> {
    const response = await api.get<SimpleUser[]>(`/users/${userId}/followers`);
    return response.data;
  },

  async getFollowing(userId: string): Promise<SimpleUser[]> {
    const response = await api.get<SimpleUser[]>(`/users/${userId}/following`);
    return response.data;
  },

  async uploadAvatar(file: File): Promise<{ profileImageUrl: string }> {
    const formData = new FormData();
    formData.append('avatar', file);
    const response = await api.post<{ profileImageUrl: string }>('/users/me/avatar', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },
};
