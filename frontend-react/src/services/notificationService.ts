import api from './api';
import { Notification, PagedResult } from '../types';

export const notificationService = {
  async getNotifications(page: number = 1, pageSize: number = 20): Promise<PagedResult<Notification>> {
    const response = await api.get<PagedResult<Notification>>('/notifications', {
      params: { page, pageSize },
    });
    return response.data;
  },

  async markAsRead(notificationId: string): Promise<void> {
    await api.put(`/notifications/${notificationId}/read`);
  },
};
