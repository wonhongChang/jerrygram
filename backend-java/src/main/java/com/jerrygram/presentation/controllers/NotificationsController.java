package com.jerrygram.presentation.controllers;

import com.jerrygram.application.commands.notifications.MarkNotificationAsReadCommand;
import com.jerrygram.application.dtos.NotificationDto;
import com.jerrygram.application.dtos.PagedResult;
import com.jerrygram.application.interfaces.ICommandHandler;
import com.jerrygram.application.interfaces.IQueryHandler;
import com.jerrygram.application.queries.notifications.GetNotificationsQuery;
import com.jerrygram.domain.entities.User;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.*;

import java.util.UUID;

@RestController
@RequestMapping("/api/notifications")
@RequiredArgsConstructor
@Slf4j
public class NotificationsController {

    private final IQueryHandler<GetNotificationsQuery, PagedResult<NotificationDto>> getNotificationsQueryHandler;
    private final ICommandHandler<MarkNotificationAsReadCommand, Boolean> markNotificationAsReadCommandHandler;

    @GetMapping
    public ResponseEntity<PagedResult<NotificationDto>> getNotifications(
            @RequestParam(defaultValue = "0") int page,
            @RequestParam(defaultValue = "10") int size,
            @RequestParam(defaultValue = "false") boolean unreadOnly,
            Authentication authentication) {
        
        User currentUser = (User) authentication.getPrincipal();
        log.info("Getting notifications for user: {}, unreadOnly: {}", 
                currentUser.getUsername(), unreadOnly);
        
        var query = new GetNotificationsQuery(currentUser.getId(), page, size, unreadOnly);
        var result = getNotificationsQueryHandler.handle(query);
        
        log.info("Retrieved {} notifications for user", result.getItems().size());
        return ResponseEntity.ok(result);
    }

    @PutMapping("/{notificationId}/read")
    public ResponseEntity<Boolean> markAsRead(
            @PathVariable UUID notificationId,
            Authentication authentication) {
        
        User currentUser = (User) authentication.getPrincipal();
        log.info("User {} marking notification {} as read", 
                currentUser.getUsername(), notificationId);
        
        var command = new MarkNotificationAsReadCommand(notificationId, currentUser.getId());
        var result = markNotificationAsReadCommandHandler.handle(command);
        
        log.info("Notification {} marked as read", notificationId);
        return ResponseEntity.ok(result);
    }
}