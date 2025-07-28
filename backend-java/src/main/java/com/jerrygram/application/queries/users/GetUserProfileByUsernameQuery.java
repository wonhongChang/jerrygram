package com.jerrygram.application.queries.users;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.UUID;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class GetUserProfileByUsernameQuery {
    private String username;
    private UUID currentUserId; // For checking follow status
}