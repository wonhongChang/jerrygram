package com.jerrygram.application.queries.posts;

import com.jerrygram.application.dtos.PostListItemDto;
import com.jerrygram.application.dtos.PagedResult;
import com.jerrygram.application.interfaces.IQueryHandler;
import com.jerrygram.application.interfaces.IRecommendClient;
import com.jerrygram.infrastructure.repositories.PostRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
@RequiredArgsConstructor
@Slf4j
public class GetExplorePostsQueryHandler implements IQueryHandler<GetExplorePostsQuery, PagedResult<PostListItemDto>> {

    private final PostRepository postRepository;
    private final IRecommendClient recommendClient;

    @Override
    public PagedResult<PostListItemDto> handle(GetExplorePostsQuery query) {
        var userId = query.getUserId();
        
        List<PostListItemDto> posts;
        if (userId == null) {
            posts = postRepository.getPopularPosts();
        } else {
            var recommended = recommendClient.getRecommendations(userId);
            if (!recommended.isEmpty()) {
                posts = recommended;
            } else {
                posts = postRepository.getPopularPostsNotFollowed(userId);
            }
        }

        return PagedResult.<PostListItemDto>builder()
                .items(posts)
                .totalCount(posts.size())
                .page(0)
                .pageSize(posts.size())
                .build();
    }
}