package com.jerrygram.application.queries.posts;

import com.jerrygram.application.dtos.PagedResult;
import com.jerrygram.application.dtos.PostListItemDto;
import com.jerrygram.application.interfaces.IQueryHandler;
import com.jerrygram.infrastructure.repositories.PostRepository;
import com.jerrygram.infrastructure.repositories.UserFollowRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
@RequiredArgsConstructor
@Slf4j
public class GetTimelineQueryHandler implements IQueryHandler<GetTimelineQuery, PagedResult<PostListItemDto>> {

    private final PostRepository postRepository;
    private final UserFollowRepository userFollowRepository;

    @Override
    public PagedResult<PostListItemDto> handle(GetTimelineQuery query) {
        var userId = query.getUserId();
        
        var followingIds = userFollowRepository.getFollowingIds(userId);

        return postRepository.getUserFeed(followingIds, userId, query.getPage(), query.getPageSize());
    }
}