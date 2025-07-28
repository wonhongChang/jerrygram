package com.jerrygram.application.queries.notifications;

import com.jerrygram.application.dtos.NotificationDto;
import com.jerrygram.application.dtos.PagedResult;
import com.jerrygram.application.dtos.SimpleUserDto;
import com.jerrygram.application.interfaces.IQueryHandler;
import com.jerrygram.infrastructure.repositories.NotificationRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Sort;
import org.springframework.stereotype.Service;

import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
@Slf4j
public class GetNotificationsQueryHandler implements IQueryHandler<GetNotificationsQuery, PagedResult<NotificationDto>> {

    private final NotificationRepository notificationRepository;

    @Override
    public PagedResult<NotificationDto> handle(GetNotificationsQuery query) {
        var userId = query.getUserId();
        var pageable = PageRequest.of(query.getPage(), query.getPageSize(), 
                Sort.by("createdAt").descending());
        
        log.info("Getting notifications for user: {}", userId);

        var notifications = notificationRepository.findByRecipientId(userId, pageable);

        var notificationDtos = notifications.getContent().stream()
                .map(this::mapToDto)
                .toList();

        return PagedResult.<NotificationDto>builder()
                .items(notificationDtos)
                .totalCount((int) notifications.getTotalElements())
                .page(query.getPage())
                .pageSize(query.getPageSize())
                .build();
    }

    private NotificationDto mapToDto(com.jerrygram.domain.entities.Notification notification) {
        SimpleUserDto fromUserDto = null;
        if (notification.getFromUser() != null) {
            var fromUser = notification.getFromUser();
            fromUserDto = SimpleUserDto.builder()
                    .id(fromUser.getId())
                    .username(fromUser.getUsername())
                    .profileImageUrl(fromUser.getProfileImageUrl())
                    .build();
        }

        return NotificationDto.builder()
                .id(notification.getId())
                .message(notification.getMessage())
                .type(notification.getType())
                .createdAt(notification.getCreatedAt())
                .isRead(notification.getIsRead())
                .fromUser(fromUserDto)
                .postId(notification.getPostId())
                .build();
    }
}