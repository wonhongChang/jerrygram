package com.jerrygram.application.queries.posts;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.UUID;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class GetTimelineQuery {
    private UUID userId;
    private int page;
    private int pageSize;
}