package com.jerrygram.application.queries.users;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.UUID;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class GetFollowersQuery {
    private UUID userId;
    private int page;
    private int size;
}