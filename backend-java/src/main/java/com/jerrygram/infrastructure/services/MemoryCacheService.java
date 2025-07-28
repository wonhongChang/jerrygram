package com.jerrygram.infrastructure.services;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.jerrygram.application.interfaces.ICacheService;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;

import java.time.Duration;
import java.time.LocalDateTime;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.regex.Pattern;

@Service
@RequiredArgsConstructor
@Slf4j
public class MemoryCacheService implements ICacheService {

    private final Map<String, CacheEntry> cache = new ConcurrentHashMap<>();
    private final ObjectMapper objectMapper;

    @Override
    public <T> void set(String key, T value, Duration expiration) {
        LocalDateTime expiryTime = LocalDateTime.now().plus(expiration);
        CacheEntry entry = new CacheEntry(value, expiryTime);
        cache.put(key, entry);
        log.debug("Set memory cache for key: {} with expiration: {}", key, expiration);
    }

    @Override
    public <T> java.util.Optional<T> get(String key, Class<T> type) {
        CacheEntry entry = cache.get(key);
        if (entry == null) {
            log.debug("Memory cache miss for key: {}", key);
            return java.util.Optional.empty();
        }

        if (entry.isExpired()) {
            cache.remove(key);
            log.debug("Memory cache entry expired for key: {}", key);
            return java.util.Optional.empty();
        }

        try {
            if (type.isInstance(entry.getValue())) {
                log.debug("Memory cache hit for key: {}", key);
                return java.util.Optional.of(type.cast(entry.getValue()));
            } else {
                String json = objectMapper.writeValueAsString(entry.getValue());
                T value = objectMapper.readValue(json, type);
                log.debug("Memory cache hit with conversion for key: {}", key);
                return java.util.Optional.of(value);
            }
        } catch (Exception e) {
            log.warn("Failed to convert cached value for key: {}", key, e);
            cache.remove(key);
            return java.util.Optional.empty();
        }
    }

    @Override
    public void delete(String key) {
        CacheEntry removed = cache.remove(key);
        log.debug("Deleted memory cache key: {} (existed: {})", key, removed != null);
    }

    @Override
    public void deleteByPattern(String pattern) {
        String regexPattern = pattern.replace("*", ".*");
        Pattern compiledPattern = Pattern.compile(regexPattern);
        
        int initialSize = cache.size();
        cache.entrySet().removeIf(entry -> 
            compiledPattern.matcher(entry.getKey()).matches());
        long removedCount = initialSize - cache.size();
        
        log.debug("Deleted {} memory cache keys matching pattern: {}", removedCount, pattern);
    }

    @Override
    public boolean exists(String key) {
        CacheEntry entry = cache.get(key);
        if (entry == null) {
            return false;
        }
        
        if (entry.isExpired()) {
            cache.remove(key);
            return false;
        }
        
        return true;
    }

    @Override
    public <T> void set(String key, T value) {
        set(key, value, Duration.ofHours(1)); // Default TTL
    }

    @Override
    public void expire(String key, Duration ttl) {
        CacheEntry entry = cache.get(key);
        if (entry != null) {
            LocalDateTime newExpiry = LocalDateTime.now().plus(ttl);
            cache.put(key, new CacheEntry(entry.getValue(), newExpiry));
        }
    }

    private static class CacheEntry {
        private final Object value;
        private final LocalDateTime expiryTime;

        public CacheEntry(Object value, LocalDateTime expiryTime) {
            this.value = value;
            this.expiryTime = expiryTime;
        }

        public Object getValue() {
            return value;
        }

        public boolean isExpired() {
            return LocalDateTime.now().isAfter(expiryTime);
        }
    }
}