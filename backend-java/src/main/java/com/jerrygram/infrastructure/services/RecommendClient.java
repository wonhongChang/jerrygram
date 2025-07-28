package com.jerrygram.infrastructure.services;

import com.jerrygram.application.dtos.PostListItemDto;
import com.jerrygram.application.interfaces.IRecommendClient;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.core.ParameterizedTypeReference;
import org.springframework.http.HttpMethod;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Service;
import org.springframework.web.client.RestTemplate;

import java.time.Duration;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

@Service
@RequiredArgsConstructor
@Slf4j
public class RecommendClient implements IRecommendClient {

    private final RestTemplate restTemplate;

    @Value("${recommendation.service.url:http://localhost:3001}")
    private String recommendationServiceUrl;

    @Value("${recommendation.service.timeout:3000}")
    private Duration timeout;

    @Override
    public List<PostListItemDto> getRecommendations(UUID userId) {
        try {
            String url = recommendationServiceUrl + "/recommend?userId=" + userId;
            
            log.info("Calling recommendation service for user: {} at {}", userId, url);
            
            ResponseEntity<List<PostListItemDto>> response = restTemplate.exchange(
                    url,
                    HttpMethod.GET,
                    null,
                    new ParameterizedTypeReference<List<PostListItemDto>>() {}
            );
            
            List<PostListItemDto> recommendations = response.getBody();
            if (recommendations != null) {
                log.info("Received {} recommendations for user: {}", recommendations.size(), userId);
                return recommendations;
            } else {
                log.warn("Received null recommendations for user: {}", userId);
                return new ArrayList<>();
            }
            
        } catch (Exception e) {
            log.error("Failed to get recommendations for user: {}", userId, e);
            return new ArrayList<>();
        }
    }
}