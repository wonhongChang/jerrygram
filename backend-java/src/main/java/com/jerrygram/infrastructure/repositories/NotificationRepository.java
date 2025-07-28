package com.jerrygram.infrastructure.repositories;

import com.jerrygram.domain.entities.Notification;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Modifying;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.UUID;

@Repository
public interface NotificationRepository extends JpaRepository<Notification, UUID> {
    
    /**
     * Find notifications by recipient
     */
    Page<Notification> findByRecipientId(UUID recipientId, Pageable pageable);
    
    /**
     * Find unread notifications by recipient
     */
    Page<Notification> findByRecipientIdAndIsReadFalse(UUID recipientId, Pageable pageable);
    
    /**
     * Count unread notifications
     */
    @Query("SELECT COUNT(n) FROM Notification n WHERE n.recipientId = :recipientId AND n.isRead = false")
    Long countUnreadNotifications(@Param("recipientId") UUID recipientId);
    
    /**
     * Mark all notifications as read for a user
     */
    @Modifying
    @Query("UPDATE Notification n SET n.isRead = true WHERE n.recipientId = :recipientId AND n.isRead = false")
    int markAllAsRead(@Param("recipientId") UUID recipientId);
    
    /**
     * Mark notification as read
     */
    @Modifying
    @Query("UPDATE Notification n SET n.isRead = true WHERE n.id = :notificationId")
    int markAsRead(@Param("notificationId") UUID notificationId);
    
    /**
     * Delete old notifications (older than certain days)
     */
    @Modifying
    @Query("DELETE FROM Notification n WHERE n.createdAt < :cutoffDate")
    int deleteOldNotifications(@Param("cutoffDate") java.time.LocalDateTime cutoffDate);
    
    /**
     * Find notifications by type and related entities
     */
    @Query("SELECT n FROM Notification n WHERE n.recipientId = :recipientId AND n.type = :type AND n.postId = :postId")
    List<Notification> findByRecipientAndTypeAndPostId(
            @Param("recipientId") UUID recipientId, 
            @Param("type") com.jerrygram.domain.enums.NotificationType type, 
            @Param("postId") UUID postId);
    
    /**
     * Delete comment notification
     */
    @Modifying
    @Query("DELETE FROM Notification n WHERE n.postId = :postId AND n.fromUserId = :fromUserId AND n.type = 0")
    int deleteCommentNotification(@Param("postId") UUID postId, @Param("fromUserId") UUID fromUserId);
}