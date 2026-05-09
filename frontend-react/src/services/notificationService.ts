import api, { getPaginationParams } from './api';
import { Notification, PagedResult } from '../types';
import { normalizeNotification, normalizePagedResult } from '../utils/apiData';

export const notificationService = {
  async getNotifications(page: number = 1, pageSize: number = 20): Promise<PagedResult<Notification>> {
    const response = await api.get('/notifications', {
      params: getPaginationParams(page, pageSize),
    });
    return normalizePagedResult(response.data, normalizeNotification);
  },

  async markAsRead(notificationId: string): Promise<void> {
    await api.put(`/notifications/${notificationId}/read`);
  },
};
