package com.jerrygram.presentation.controllers;

import lombok.extern.slf4j.Slf4j;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.time.LocalDateTime;
import java.util.Map;

@RestController
@RequestMapping("/api/health")
@Slf4j
public class HealthController {

    @GetMapping
    public ResponseEntity<Map<String, Object>> health() {
        log.debug("Health check requested");
        
        Map<String, Object> response = Map.of(
                "status", "healthy",
                "timestamp", LocalDateTime.now(),
                "service", "jerrygram-java",
                "version", "1.0.0"
        );
        
        return ResponseEntity.ok(response);
    }
}