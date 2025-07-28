package com.jerrygram.application.queries.posts;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.UUID;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class GetPostLikesQuery {
    private UUID postId;
    private int page;
    private int pageSize;
}