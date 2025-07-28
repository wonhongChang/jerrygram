package com.jerrygram.application.queries.posts;

import com.jerrygram.application.dtos.PagedResult;
import com.jerrygram.application.dtos.SimpleUserDto;
import com.jerrygram.application.interfaces.IQueryHandler;
import com.jerrygram.infrastructure.repositories.PostLikeRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.data.domain.PageRequest;
import org.springframework.stereotype.Service;

@Service
@RequiredArgsConstructor
@Slf4j
public class GetPostLikesQueryHandler implements IQueryHandler<GetPostLikesQuery, PagedResult<SimpleUserDto>> {

    private final PostLikeRepository postLikeRepository;

    @Override
    public PagedResult<SimpleUserDto> handle(GetPostLikesQuery query) {
        var pageable = PageRequest.of(query.getPage(), query.getPageSize());
        var users = postLikeRepository.getPostLikesUsers(query.getPostId(), pageable);
        var totalCount = postLikeRepository.countByPostId(query.getPostId());
        
        return PagedResult.<SimpleUserDto>builder()
                .items(users)
                .totalCount((int) totalCount)
                .page(query.getPage())
                .pageSize(query.getPageSize())
                .build();
    }
}