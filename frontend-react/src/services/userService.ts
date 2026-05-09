import api from './api';
import { User, SimpleUser } from '../types';
import { normalizeSimpleUser, normalizeUser, resolveAssetUrl } from '../utils/apiData';

export const userService = {
  async getCurrentUser(): Promise<User> {
    const response = await api.get('/users/me');
    return normalizeUser(response.data);
  },

  async getUserProfile(username: string): Promise<User> {
    try {
      const response = await api.get(`/users/${username}`);
      return normalizeUser(response.data);
    } catch (error: any) {
      if (error?.response?.status !== 404) throw error;
      const response = await api.get(`/users/profile/${username}`);
      return normalizeUser(response.data);
    }
  },

  async followUser(userId: string): Promise<void> {
    await api.post(`/users/${userId}/follow`);
  },

  async unfollowUser(userId: string): Promise<void> {
    try {
      await api.delete(`/users/${userId}/unfollow`);
    } catch (error: any) {
      if (error?.response?.status !== 404 && error?.response?.status !== 405) throw error;
      await api.post(`/users/${userId}/follow`);
    }
  },

  async getFollowers(userId: string): Promise<SimpleUser[]> {
    const response = await api.get(`/users/${userId}/followers`);
    return Array.isArray(response.data) ? response.data.map(normalizeSimpleUser) : [];
  },

  async getFollowing(userId: string): Promise<SimpleUser[]> {
    try {
      const response = await api.get(`/users/${userId}/following`);
      return Array.isArray(response.data) ? response.data.map(normalizeSimpleUser) : [];
    } catch (error: any) {
      if (error?.response?.status !== 404) throw error;
      const response = await api.get(`/users/${userId}/followings`);
      return Array.isArray(response.data) ? response.data.map(normalizeSimpleUser) : [];
    }
  },

  async uploadAvatar(file: File): Promise<{ profileImageUrl: string }> {
    const formData = new FormData();
    formData.append('avatar', file);
    const response = await api.post<{ profileImageUrl?: string; imageUrl?: string }>('/users/me/avatar', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return {
      profileImageUrl: resolveAssetUrl(response.data.profileImageUrl || response.data.imageUrl) || '',
    };
  },
};
