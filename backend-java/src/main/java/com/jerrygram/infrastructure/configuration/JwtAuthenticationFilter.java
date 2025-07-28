package com.jerrygram.infrastructure.configuration;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.SerializationFeature;
import com.fasterxml.jackson.datatype.jsr310.JavaTimeModule;
import com.jerrygram.application.dtos.ErrorResponse;
import com.jerrygram.application.interfaces.IJwtService;
import com.jerrygram.domain.entities.User;
import com.jerrygram.infrastructure.repositories.UserRepository;
import io.jsonwebtoken.ExpiredJwtException;
import io.jsonwebtoken.MalformedJwtException;
import jakarta.servlet.FilterChain;
import jakarta.servlet.ServletException;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.security.web.authentication.WebAuthenticationDetailsSource;
import org.springframework.stereotype.Component;
import org.springframework.web.filter.OncePerRequestFilter;

import java.io.IOException;
import java.time.LocalDateTime;
import java.util.Collections;
import java.util.UUID;

@Component
@RequiredArgsConstructor
@Slf4j
public class JwtAuthenticationFilter extends OncePerRequestFilter {

    private final IJwtService jwtService;
    private final UserRepository userRepository;
    private final ObjectMapper objectMapper = createObjectMapper();
    
    private static ObjectMapper createObjectMapper() {
        ObjectMapper mapper = new ObjectMapper();
        mapper.registerModule(new JavaTimeModule());
        mapper.disable(SerializationFeature.WRITE_DATES_AS_TIMESTAMPS);
        return mapper;
    }

    @Override
    protected void doFilterInternal(HttpServletRequest request, HttpServletResponse response, FilterChain filterChain)
            throws ServletException, IOException {

        final String authHeader = request.getHeader("Authorization");
        final String requestURI = request.getRequestURI();
        
        log.debug("Processing request: {} with auth header: {}", requestURI, authHeader != null ? "Bearer ***" : "null");
        
        if (authHeader == null || !authHeader.startsWith("Bearer ")) {
            log.debug("No valid Bearer token found for: {}", requestURI);
            filterChain.doFilter(request, response);
            return;
        }

        try {
            final String jwt = authHeader.substring(7);
            final UUID userId = jwtService.extractUserId(jwt);
            log.debug("Extracted userId from JWT: {}", userId);

            if (userId != null && SecurityContextHolder.getContext().getAuthentication() == null) {
                User user = userRepository.findById(userId).orElse(null);
                log.debug("Found user in database: {}", user != null ? user.getUsername() : "null");
                
                if (user != null && jwtService.validateToken(jwt)) {
                    UsernamePasswordAuthenticationToken authToken = new UsernamePasswordAuthenticationToken(
                            user, null, Collections.emptyList());
                    authToken.setDetails(new WebAuthenticationDetailsSource().buildDetails(request));
                    SecurityContextHolder.getContext().setAuthentication(authToken);
                    
                    log.debug("Successfully authenticated user: {} for request: {}", user.getUsername(), requestURI);
                } else {
                    log.debug("Token validation failed for user: {}", userId);
                }
            } else if (userId == null) {
                log.warn("Failed to extract userId from JWT token");
            } else {
                log.debug("Authentication already exists in SecurityContext");
            }
        } catch (Exception e) {
            log.warn("JWT authentication failed for {}: {}", requestURI, e.getMessage());
            // Let Spring Security's AuthenticationEntryPoint handle the exception
            // Continue filter chain without sending direct response
        }

        filterChain.doFilter(request, response);
    }
    
    private void sendErrorResponse(HttpServletResponse response, int status, String error, String message) {
        try {
            ErrorResponse errorResponse = ErrorResponse.builder()
                    .timestamp(LocalDateTime.now())
                    .status(status)
                    .error(error)
                    .message(message)
                    .path(null)
                    .build();
            
            response.setStatus(status);
            response.setContentType("application/json");
            response.setCharacterEncoding("UTF-8");
            response.getWriter().write(objectMapper.writeValueAsString(errorResponse));
            response.getWriter().flush();
        } catch (Exception ex) {
            log.error("Failed to send JWT error response", ex);
            // Fallback: simple JSON response
            try {
                response.setStatus(status);
                response.setContentType("application/json");
                response.setCharacterEncoding("UTF-8");
                response.getWriter().write("{\"error\":\"" + error + "\",\"message\":\"" + message + "\"}");
                response.getWriter().flush();
            } catch (Exception fallbackEx) {
                log.error("Failed to send fallback error response", fallbackEx);
            }
        }
    }
}