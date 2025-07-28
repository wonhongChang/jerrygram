package com.jerrygram.application.queries.users;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class GetUserProfileQuery {
    private String username;
    private java.util.UUID userId;
    private java.util.UUID currentUserId;
    
    // Constructor for username-based queries
    public GetUserProfileQuery(String username) {
        this.username = username;
    }
    
    // Constructor for userId-based queries  
    public GetUserProfileQuery(java.util.UUID userId, java.util.UUID currentUserId) {
        this.userId = userId;
        this.currentUserId = currentUserId;
    }
}