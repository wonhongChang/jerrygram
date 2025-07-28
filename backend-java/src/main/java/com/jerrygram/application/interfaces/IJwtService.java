package com.jerrygram.application.interfaces;

import com.jerrygram.domain.entities.User;
import io.jsonwebtoken.Claims;

import java.util.UUID;

/**
 * JWT service interface for token management
 */
public interface IJwtService {
    
    /**
     * Generate JWT token for user
     * @param user User entity
     * @return JWT token string
     */
    String generateToken(User user);
    
    /**
     * Extract user ID from token
     * @param token JWT token
     * @return User ID
     */
    UUID extractUserId(String token);
    
    /**
     * Extract username from token
     * @param token JWT token
     * @return Username
     */
    String extractUsername(String token);
    
    /**
     * Validate token
     * @param token JWT token
     * @return true if token is valid
     */
    boolean validateToken(String token);
    
    /**
     * Extract all claims from token
     * @param token JWT token
     * @return Claims object
     */
    Claims extractClaims(String token);
    
    /**
     * Check if token is expired
     * @param token JWT token
     * @return true if token is expired
     */
    boolean isTokenExpired(String token);
}