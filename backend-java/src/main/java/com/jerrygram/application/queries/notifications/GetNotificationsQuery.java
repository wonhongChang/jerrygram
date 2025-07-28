package com.jerrygram.application.queries.notifications;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.UUID;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class GetNotificationsQuery {
    private UUID userId;
    private int page;
    private int pageSize;
    private boolean unreadOnly;
    
    // Constructor without unreadOnly for backward compatibility
    public GetNotificationsQuery(UUID userId, int page, int pageSize) {
        this.userId = userId;
        this.page = page;
        this.pageSize = pageSize;
        this.unreadOnly = false;
    }
}