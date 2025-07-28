package com.jerrygram.application.queries.posts;

import com.jerrygram.application.dtos.PagedResult;
import com.jerrygram.application.dtos.PostListItemDto;
import com.jerrygram.application.interfaces.ICacheService;
import com.jerrygram.application.interfaces.IQueryHandler;
import com.jerrygram.infrastructure.repositories.PostRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;

import java.time.Duration;

@Service
@RequiredArgsConstructor
@Slf4j
public class GetPublicPostsQueryHandler implements IQueryHandler<GetPublicPostsQuery, PagedResult<PostListItemDto>> {

    private final PostRepository postRepository;
    private final ICacheService cacheService;

    @Override
    public PagedResult<PostListItemDto> handle(GetPublicPostsQuery query) {
        var cacheKey = "public_posts_page_" + query.getPage() + "_" + query.getPageSize() + "_" + query.getCurrentUserId();
        
        var cached = cacheService.get(cacheKey, PagedResult.class);
        if (cached.isPresent()) {
            log.debug("Public posts page {} retrieved from cache", query.getPage());
            @SuppressWarnings("unchecked")
            PagedResult<PostListItemDto> result = (PagedResult<PostListItemDto>) cached.get();
            return result;
        }

        var result = postRepository.getPublicPosts(query.getCurrentUserId(), query.getPage(), query.getPageSize());

        // Cache for 15 minutes
        cacheService.set(cacheKey, result, Duration.ofMinutes(15));

        log.info("Retrieved {} public posts for page {}", result.getItems().size(), query.getPage());
        return result;
    }
}