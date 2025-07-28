package com.jerrygram.infrastructure.services;

import com.jerrygram.application.interfaces.ICacheService;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.context.annotation.Primary;
import org.springframework.stereotype.Service;

import java.time.Duration;

@Service
@Primary
@ConditionalOnProperty(name = "cache.use-redis", havingValue = "true")
@RequiredArgsConstructor
@Slf4j
public class HybridCacheService implements ICacheService {

    private final RedisCacheService redisCacheService;
    private final MemoryCacheService memoryCacheService;

    @Override
    public <T> void set(String key, T value, Duration expiration) {
        try {
            redisCacheService.set(key, value, expiration);
            log.debug("Set cache in Redis for key: {}", key);
        } catch (Exception e) {
            log.warn("Redis set failed for key: {}, falling back to memory cache", key, e);
            memoryCacheService.set(key, value, expiration);
        }
    }

    @Override
    public <T> java.util.Optional<T> get(String key, Class<T> type) {
        try {
            java.util.Optional<T> value = redisCacheService.get(key, type);
            if (value.isPresent()) {
                log.debug("Cache hit in Redis for key: {}", key);
                return value;
            }
        } catch (Exception e) {
            log.warn("Redis get failed for key: {}, trying memory cache", key, e);
        }

        java.util.Optional<T> value = memoryCacheService.get(key, type);
        if (value.isPresent()) {
            log.debug("Cache hit in memory for key: {}", key);
        } else {
            log.debug("Cache miss for key: {}", key);
        }
        return value;
    }

    @Override
    public void delete(String key) {
        try {
            redisCacheService.delete(key);
        } catch (Exception e) {
            log.warn("Redis delete failed for key: {}", key, e);
        }
        
        memoryCacheService.delete(key);
    }

    @Override
    public void deleteByPattern(String pattern) {
        try {
            redisCacheService.deleteByPattern(pattern);
        } catch (Exception e) {
            log.warn("Redis deleteByPattern failed for pattern: {}", pattern, e);
        }
        
        memoryCacheService.deleteByPattern(pattern);
    }

    @Override
    public boolean exists(String key) {
        try {
            return redisCacheService.exists(key);
        } catch (Exception e) {
            log.warn("Redis exists check failed for key: {}, checking memory cache", key, e);
            return memoryCacheService.exists(key);
        }
    }

    @Override
    public <T> void set(String key, T value) {
        set(key, value, Duration.ofHours(1)); // Default TTL
    }

    @Override
    public void expire(String key, Duration ttl) {
        try {
            redisCacheService.expire(key, ttl);
        } catch (Exception e) {
            log.warn("Redis expire failed for key: {}", key, e);
        }
        
        memoryCacheService.expire(key, ttl);
    }
}