package com.jerrygram.application.queries.comments;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.UUID;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class GetPostCommentsQuery {
    private UUID postId;
    private int page;
    private int size;
}