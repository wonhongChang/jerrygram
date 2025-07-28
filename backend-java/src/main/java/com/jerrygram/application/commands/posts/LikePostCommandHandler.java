package com.jerrygram.application.commands.posts;

import com.jerrygram.application.interfaces.ICommandHandler;
import com.jerrygram.domain.entities.PostLike;
import com.jerrygram.infrastructure.repositories.PostLikeRepository;
import com.jerrygram.infrastructure.repositories.PostRepository;
import com.jerrygram.infrastructure.repositories.UserRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

@Service
@RequiredArgsConstructor
@Slf4j
public class LikePostCommandHandler implements ICommandHandler<LikePostCommand, Boolean> {

    private final PostLikeRepository postLikeRepository;
    private final PostRepository postRepository;
    private final UserRepository userRepository;

    @Override
    @Transactional
    public Boolean handle(LikePostCommand command) {
        var postId = command.getPostId();
        var userId = command.getUserId();
        
        log.info("Processing like for post: {} by user: {}", postId, userId);

        var existingLike = postLikeRepository.findByPostIdAndUserId(postId, userId);
        
        if (existingLike.isPresent()) {
            // Unlike the post
            postLikeRepository.delete(existingLike.get());
            
            log.info("User {} unliked post {}", userId, postId);
            return false;
        } else {
            // Like the post
            var post = postRepository.findById(postId)
                    .orElseThrow(() -> new IllegalArgumentException("Post not found"));
            var user = userRepository.findById(userId)
                    .orElseThrow(() -> new IllegalArgumentException("User not found"));
            
            var postLike = PostLike.builder()
                    .post(post)
                    .user(user)
                    .build();
            
            postLikeRepository.save(postLike);
            
            log.info("User {} liked post {}", userId, postId);
            return true;
        }
    }
}