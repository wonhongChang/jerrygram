package com.jerrygram.application.queries.comments;

import com.jerrygram.application.dtos.CommentDto;
import com.jerrygram.application.dtos.SimpleUserDto;
import com.jerrygram.application.interfaces.IQueryHandler;
import com.jerrygram.infrastructure.repositories.CommentRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.stereotype.Service;

@Service
@RequiredArgsConstructor
@Slf4j
public class GetPostCommentsQueryHandler implements IQueryHandler<GetPostCommentsQuery, Page<CommentDto>> {

    private final CommentRepository commentRepository;

    @Override
    public Page<CommentDto> handle(GetPostCommentsQuery query) {
        var postId = query.getPostId();
        var pageable = PageRequest.of(query.getPage(), query.getSize());
        
        log.info("Getting comments for post: {}, page: {}, size: {}", postId, query.getPage(), query.getSize());

        var comments = commentRepository.findByPostIdOrderByCreatedAtDesc(postId, pageable);
        
        return comments.map(comment -> CommentDto.builder()
                .id(comment.getId())
                .content(comment.getContent())
                .createdAt(comment.getCreatedAt())
                .user(SimpleUserDto.builder()
                        .id(comment.getUser().getId())
                        .username(comment.getUser().getUsername())
                        .profileImageUrl(comment.getUser().getProfileImageUrl())
                        .build())
                .build());
    }
}