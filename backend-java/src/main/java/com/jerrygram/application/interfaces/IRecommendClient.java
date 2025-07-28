package com.jerrygram.application.interfaces;

import com.jerrygram.application.dtos.PostListItemDto;

import java.util.List;
import java.util.UUID;

public interface IRecommendClient {
    List<PostListItemDto> getRecommendations(UUID userId);
}