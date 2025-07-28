package com.jerrygram.application.commands.notifications;

import com.jerrygram.application.interfaces.ICommandHandler;
import com.jerrygram.infrastructure.repositories.NotificationRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

@Service
@RequiredArgsConstructor
@Slf4j
public class MarkNotificationAsReadCommandHandler implements ICommandHandler<MarkNotificationAsReadCommand, Boolean> {

    private final NotificationRepository notificationRepository;

    @Override
    @Transactional
    public Boolean handle(MarkNotificationAsReadCommand command) {
        var notificationId = command.getNotificationId();
        var userId = command.getUserId();
        
        log.info("Marking notification {} as read for user: {}", notificationId, userId);

        var notification = notificationRepository.findById(notificationId)
                .orElseThrow(() -> new IllegalArgumentException("Notification not found"));

        // Authorization check
        if (!notification.getRecipient().getId().equals(userId)) {
            throw new SecurityException("User not authorized to modify this notification");
        }

        notification.markAsRead();
        notificationRepository.save(notification);
        
        log.info("Notification {} marked as read", notificationId);
        return true;
    }
}