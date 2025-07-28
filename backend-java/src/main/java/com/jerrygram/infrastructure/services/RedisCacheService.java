package com.jerrygram.infrastructure.services;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.jerrygram.application.interfaces.ICacheService;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.data.redis.core.RedisTemplate;
import org.springframework.data.redis.core.ScanOptions;
import org.springframework.stereotype.Service;

import java.time.Duration;
import java.util.Set;

@Service
@RequiredArgsConstructor
@Slf4j
public class RedisCacheService implements ICacheService {

    private final RedisTemplate<String, String> redisTemplate;
    private final ObjectMapper objectMapper;

    @Override
    public <T> void set(String key, T value, Duration expiration) {
        try {
            String serializedValue = objectMapper.writeValueAsString(value);
            redisTemplate.opsForValue().set(key, serializedValue, expiration);
            log.debug("Set cache key: {} with expiration: {}", key, expiration);
        } catch (JsonProcessingException e) {
            log.error("Failed to serialize value for key: {}", key, e);
            throw new RuntimeException("Cache serialization failed", e);
        }
    }

    @Override
    public <T> java.util.Optional<T> get(String key, Class<T> type) {
        try {
            String serializedValue = redisTemplate.opsForValue().get(key);
            if (serializedValue == null) {
                log.debug("Cache miss for key: {}", key);
                return java.util.Optional.empty();
            }
            
            T value = objectMapper.readValue(serializedValue, type);
            log.debug("Cache hit for key: {}", key);
            return java.util.Optional.of(value);
        } catch (JsonProcessingException e) {
            log.error("Failed to deserialize value for key: {}", key, e);
            return java.util.Optional.empty();
        }
    }

    @Override
    public void delete(String key) {
        Boolean deleted = redisTemplate.delete(key);
        log.debug("Deleted cache key: {} (existed: {})", key, deleted);
    }

    @Override
    public void deleteByPattern(String pattern) {
        try {
            ScanOptions options = ScanOptions.scanOptions()
                    .match(pattern)
                    .count(1000)
                    .build();
            
            Set<String> keys = redisTemplate.execute((org.springframework.data.redis.core.RedisCallback<Set<String>>) connection -> {
                var cursor = connection.scan(options);
                Set<String> result = new java.util.HashSet<>();
                while (cursor.hasNext()) {
                    result.add(new String(cursor.next()));
                }
                return result;
            });
            
            if (!keys.isEmpty()) {
                redisTemplate.delete(keys);
                log.debug("Deleted {} cache keys matching pattern: {}", keys.size(), pattern);
            } else {
                log.debug("No cache keys found matching pattern: {}", pattern);
            }
        } catch (Exception e) {
            log.error("Failed to delete cache keys by pattern: {}", pattern, e);
        }
    }

    @Override
    public boolean exists(String key) {
        Boolean exists = redisTemplate.hasKey(key);
        return exists != null && exists;
    }

    @Override
    public <T> void set(String key, T value) {
        set(key, value, Duration.ofHours(1)); // Default TTL
    }

    @Override
    public void expire(String key, Duration ttl) {
        redisTemplate.expire(key, ttl);
        log.debug("Set expiration for key: {} to {}", key, ttl);
    }
}