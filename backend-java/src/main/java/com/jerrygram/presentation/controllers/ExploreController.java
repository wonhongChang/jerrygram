package com.jerrygram.presentation.controllers;

import com.jerrygram.application.dtos.PostListItemDto;
import com.jerrygram.application.dtos.PagedResult;
import com.jerrygram.application.interfaces.IQueryHandler;
import com.jerrygram.application.queries.posts.GetExplorePostsQuery;
import com.jerrygram.domain.entities.User;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/api/explore")
@RequiredArgsConstructor
@Slf4j
public class ExploreController {

    private final IQueryHandler<GetExplorePostsQuery, PagedResult<PostListItemDto>> getExplorePostsQueryHandler;

    @GetMapping
    public ResponseEntity<PagedResult<PostListItemDto>> getExploreFeed(Authentication authentication) {
        User currentUser = authentication != null ? (User) authentication.getPrincipal() : null;
        var userId = currentUser != null ? currentUser.getId() : null;
        
        log.info("Getting explore feed for user: {}", userId);
        
        var query = new GetExplorePostsQuery(userId);
        var result = getExplorePostsQueryHandler.handle(query);
        
        log.info("Retrieved {} explore posts", result.getItems().size());
        return ResponseEntity.ok(result);
    }
}