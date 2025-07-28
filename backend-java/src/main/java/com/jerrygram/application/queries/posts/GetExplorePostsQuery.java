package com.jerrygram.application.queries.posts;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.UUID;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class GetExplorePostsQuery {
    private UUID userId; // Can be null for anonymous users
}