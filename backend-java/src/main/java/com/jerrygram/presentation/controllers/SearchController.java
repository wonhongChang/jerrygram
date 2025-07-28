package com.jerrygram.presentation.controllers;

import com.jerrygram.application.dtos.SearchResultDto;
import com.jerrygram.application.interfaces.ISearchService;
import com.jerrygram.domain.entities.User;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/api/search")
@RequiredArgsConstructor
@Slf4j
public class SearchController {

    private final ISearchService searchService;

    @GetMapping
    public ResponseEntity<SearchResultDto> search(
            @RequestParam String query,
            Authentication authentication) {
        
        if (query == null || query.trim().isEmpty()) {
            return ResponseEntity.badRequest().build();
        }
        
        User currentUser = authentication != null ? (User) authentication.getPrincipal() : null;
        String userId = currentUser != null ? currentUser.getId().toString() : null;
        
        log.info("Search request: '{}' by user: {}", query, userId);
        
        var result = searchService.search(query, userId);
        
        log.info("Search completed: {} users, {} posts, {} hashtags", 
                result.getUsers().size(), result.getPosts().size(), result.getHashtags().size());
        
        return ResponseEntity.ok(result);
    }

    @GetMapping("/autocomplete")
    public ResponseEntity<SearchResultDto> autocomplete(@RequestParam String query) {
        if (query == null || query.trim().isEmpty()) {
            return ResponseEntity.badRequest().build();
        }
        
        log.info("Autocomplete request: '{}'", query);
        
        var result = searchService.autocomplete(query);
        
        log.info("Autocomplete completed: {} users, {} hashtags", 
                result.getUsers().size(), result.getHashtags().size());
        
        return ResponseEntity.ok(result);
    }
}