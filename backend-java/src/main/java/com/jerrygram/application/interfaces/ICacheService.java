package com.jerrygram.application.interfaces;

import java.time.Duration;
import java.util.Optional;

/**
 * Cache service interface for hybrid caching strategy
 */
public interface ICacheService {
    
    /**
     * Get value from cache
     * @param key Cache key
     * @param type Value type class
     * @return Optional containing the cached value if present
     */
    <T> Optional<T> get(String key, Class<T> type);
    
    /**
     * Set value in cache with default TTL
     * @param key Cache key
     * @param value Value to cache
     */
    <T> void set(String key, T value);
    
    /**
     * Set value in cache with custom TTL
     * @param key Cache key
     * @param value Value to cache
     * @param ttl Time to live
     */
    <T> void set(String key, T value, Duration ttl);
    
    /**
     * Remove value from cache
     * @param key Cache key
     */
    void delete(String key);
    
    /**
     * Remove all values matching pattern
     * @param pattern Key pattern (supports wildcards)
     */
    void deleteByPattern(String pattern);
    
    /**
     * Check if key exists in cache
     * @param key Cache key
     * @return true if key exists
     */
    boolean exists(String key);
    
    /**
     * Set TTL for existing key
     * @param key Cache key
     * @param ttl Time to live
     */
    void expire(String key, Duration ttl);
}