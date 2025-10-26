import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import Layout from '../components/layout/Layout';
import { Notification, NotificationType } from '../types';
import { notificationService } from '../services/notificationService';
import { formatDistanceToNow } from 'date-fns';
import { FiHeart, FiMessageCircle, FiUserPlus } from 'react-icons/fi';

const NotificationsPage: React.FC = () => {
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadNotifications();
  }, []);

  const loadNotifications = async () => {
    try {
      const response = await notificationService.getNotifications();
      setNotifications(response.items);
    } catch (error) {
      console.error('Failed to load notifications:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleMarkAsRead = async (notificationId: string) => {
    try {
      await notificationService.markAsRead(notificationId);
      setNotifications(notifications.map(n =>
        n.id === notificationId ? { ...n, isRead: true } : n
      ));
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
        <div className="flex justify-center items-center h-64">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="max-w-2xl mx-auto">
        <div className="bg-white border border-gray-300 rounded-lg">
          <div className="border-b border-gray-300 px-6 py-4">
            <h1 className="text-2xl font-semibold">Notifications</h1>
          </div>

          {notifications.length === 0 ? (
            <div className="text-center py-12">
              <p className="text-gray-500 text-lg">No notifications yet</p>
              <p className="text-gray-400 text-sm mt-2">
                When someone likes or comments on your posts, you'll see them here
              </p>
            </div>
          ) : (
            <div className="divide-y divide-gray-200">
              {notifications.map((notification) => (
                <Link
                  key={notification.id}
                  to={getNotificationLink(notification)}
                  onClick={() => !notification.isRead && handleMarkAsRead(notification.id)}
                  className={`flex items-center gap-4 p-4 hover:bg-gray-50 transition-colors ${
                    !notification.isRead ? 'bg-blue-50' : ''
                  }`}
                >
                  {/* User Avatar */}
                  <div className="flex-shrink-0 relative">
                    {notification.fromUser.profileImageUrl ? (
                      <img
                        src={notification.fromUser.profileImageUrl}
                        alt={notification.fromUser.username}
                        className="w-12 h-12 rounded-full object-cover"
                      />
                    ) : (
                      <div className="w-12 h-12 rounded-full bg-gray-300" />
                    )}
                    <div className="absolute -bottom-1 -right-1 bg-white rounded-full p-1">
                      {getNotificationIcon(notification.type)}
                    </div>
                  </div>

                  {/* Notification Content */}
                  <div className="flex-1 min-w-0">
                    <p className="text-sm">
                      <span className="font-semibold">{notification.fromUser.username}</span>{' '}
                      <span className="text-gray-700">{notification.message}</span>
                    </p>
                    <p className="text-xs text-gray-400 mt-1">
                      {formatDistanceToNow(new Date(notification.createdAt), { addSuffix: true })}
                    </p>
                  </div>

                  {/* Unread Indicator */}
                  {!notification.isRead && (
                    <div className="w-2 h-2 bg-primary rounded-full flex-shrink-0"></div>
                  )}
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
