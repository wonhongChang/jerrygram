import React, { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import Layout from '../components/layout/Layout';
import Avatar from '../components/ui/Avatar';
import EmptyState from '../components/ui/EmptyState';
import ErrorState from '../components/ui/ErrorState';
import LoadingSpinner from '../components/ui/LoadingSpinner';
import { Notification, NotificationType } from '../types';
import { notificationService } from '../services/notificationService';
import { formatRelativeTime, getApiErrorMessage } from '../utils/apiData';
import { FiHeart, FiMessageCircle, FiUserPlus } from 'react-icons/fi';

const NotificationsPage: React.FC = () => {
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const loadNotifications = useCallback(async () => {
    try {
      setLoading(true);
      setError('');
      const response = await notificationService.getNotifications();
      setNotifications(response.items);
    } catch (error) {
      setError(getApiErrorMessage(error, 'Failed to load notifications'));
      console.error('Failed to load notifications:', error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadNotifications();
  }, [loadNotifications]);

  const handleMarkAsRead = async (notificationId: string) => {
    try {
      await notificationService.markAsRead(notificationId);
      setNotifications((current) =>
        current.map((notification) =>
          notification.id === notificationId ? { ...notification, isRead: true } : notification
        )
      );
    } catch (error) {
      console.error('Failed to mark as read:', error);
    }
  };

  const getNotificationIcon = (type: NotificationType) => {
    switch (type) {
      case NotificationType.LIKE:
        return <FiHeart className="text-red-500" size={20} />;
      case NotificationType.COMMENT:
        return <FiMessageCircle className="text-blue-500" size={20} />;
      case NotificationType.FOLLOW:
        return <FiUserPlus className="text-green-500" size={20} />;
      default:
        return null;
    }
  };

  const getNotificationLink = (notification: Notification) => {
    if (notification.postId) {
      return `/p/${notification.postId}`;
    }
    return `/${notification.fromUser.username}`;
  };

  if (loading) {
    return (
      <Layout>
        <LoadingSpinner label="Loading notifications" className="h-64" />
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="mx-auto max-w-2xl">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-gray-950">Notifications</h1>
        </div>

        <div className="overflow-hidden rounded-lg border border-gray-200 bg-white shadow-sm">
          <div className="border-b border-gray-200 px-5 py-4">
            <h2 className="text-sm font-semibold text-gray-950">Activity</h2>
          </div>

          {error ? (
            <div className="p-4">
              <ErrorState message={error} onRetry={loadNotifications} />
            </div>
          ) : notifications.length === 0 ? (
            <EmptyState title="No notifications yet" />
          ) : (
            <div className="divide-y divide-gray-100">
              {notifications.map((notification) => (
                <Link
                  key={notification.id}
                  to={getNotificationLink(notification)}
                  onClick={() => !notification.isRead && handleMarkAsRead(notification.id)}
                  className={`flex items-center gap-4 p-4 transition-colors hover:bg-gray-50 ${
                    !notification.isRead ? 'bg-blue-50/70' : ''
                  }`}
                >
                  <div className="flex-shrink-0 relative">
                    <Avatar src={notification.fromUser.profileImageUrl} username={notification.fromUser.username} size="lg" />
                    <div className="absolute -bottom-1 -right-1 bg-white rounded-full p-1">
                      {getNotificationIcon(notification.type)}
                    </div>
                  </div>

                  <div className="flex-1 min-w-0">
                    <p className="text-sm">
                      <span className="font-semibold">{notification.fromUser.username}</span>{' '}
                      <span className="text-gray-700">{notification.message}</span>
                    </p>
                    <p className="text-xs text-gray-400 mt-1">{formatRelativeTime(notification.createdAt)}</p>
                  </div>

                  {!notification.isRead && <div className="h-2 w-2 flex-shrink-0 rounded-full bg-primary" />}
                </Link>
              ))}
            </div>
          )}
        </div>
      </div>
    </Layout>
  );
};

export default NotificationsPage;
