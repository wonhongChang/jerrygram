package com.jerrygram.application.queries.posts;

import com.jerrygram.application.dtos.PostListItemDto;
import com.jerrygram.application.interfaces.ICacheService;
import com.jerrygram.application.interfaces.IQueryHandler;
import com.jerrygram.domain.enums.PostVisibility;
import com.jerrygram.infrastructure.repositories.PostRepository;
import com.jerrygram.infrastructure.repositories.UserFollowRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;

import java.time.Duration;
import java.util.NoSuchElementException;

@Service
@RequiredArgsConstructor
@Slf4j
public class GetPostByIdQueryHandler implements IQueryHandler<GetPostByIdQuery, PostListItemDto> {

    private final PostRepository postRepository;
    private final ICacheService cacheService;
    private final UserFollowRepository userFollowRepository;

    @Override
    public PostListItemDto handle(GetPostByIdQuery query) {
        var postId = query.getPostId();
        var currentUserId = query.getCurrentUserId();
        
        var cacheKey = "post_details_" + postId + "_" + currentUserId;
        
        var cached = cacheService.get(cacheKey, PostListItemDto.class);
        if (cached.isPresent()) {
            log.debug("Post {} retrieved from cache", postId);
            return cached.get();
        }

        // Get the post entity to check visibility rules first
        var post = postRepository.getPostWithUserAndLikes(postId);
        
        if (post == null) {
            throw new NoSuchElementException("Post not found");
        }

        // Check visibility permissions
        if (post.getVisibility() == PostVisibility.Private && !post.getUserId().equals(currentUserId)) {
            throw new SecurityException("You don't have permission to view this post");
        }

        if (post.getVisibility() == PostVisibility.FollowersOnly && currentUserId != null && !post.getUserId().equals(currentUserId)) {
            var isFollowing = userFollowRepository.existsByFollowerIdAndFollowingId(currentUserId, post.getUserId());
            
            if (!isFollowing) {
                throw new SecurityException("You don't have permission to view this post");
            }
        }

        // Get the DTO after permission checks pass
        var result = postRepository.getPostDto(postId, currentUserId);
        
        if (result == null) {
            throw new NoSuchElementException("Post not found");
        }

        // Cache for 30 minutes
        cacheService.set(cacheKey, result, Duration.ofMinutes(30));

        log.info("Post {} retrieved successfully", postId);
        return result;
    }
}