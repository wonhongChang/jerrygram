package com.jerrygram.application.commands.posts;

import com.jerrygram.application.interfaces.IBlobService;
import com.jerrygram.application.interfaces.ICommandHandler;
import com.jerrygram.application.interfaces.IElasticService;
import com.jerrygram.infrastructure.repositories.PostRepository;
import com.jerrygram.infrastructure.repositories.UserRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

@Service
@RequiredArgsConstructor
@Slf4j
public class DeletePostCommandHandler implements ICommandHandler<DeletePostCommand, Boolean> {

    private final PostRepository postRepository;
    private final UserRepository userRepository;
    private final IBlobService blobService;
    private final IElasticService elasticService;

    @Override
    @Transactional
    public Boolean handle(DeletePostCommand command) {
        var postId = command.getPostId();
        var userId = command.getUserId();
        
        log.info("Deleting post: {} by user: {}", postId, userId);

        var post = postRepository.findById(postId)
                .orElseThrow(() -> new IllegalArgumentException("Post not found"));

        // Authorization check - only post owner can delete
        if (!post.getUser().getId().equals(userId)) {
            throw new SecurityException("User not authorized to delete this post");
        }

        // Delete image from blob storage if exists
        if (post.getImageUrl() != null && !post.getImageUrl().isEmpty()) {
            try {
                blobService.delete(post.getImageUrl());
                log.info("Image deleted from blob storage: {}", post.getImageUrl());
            } catch (Exception e) {
                log.warn("Failed to delete image from blob storage: {}", post.getImageUrl(), e);
            }
        }

        // Remove from Elasticsearch
        try {
            elasticService.deletePost(postId.toString());
            log.info("Post removed from Elasticsearch: {}", postId);
        } catch (Exception e) {
            log.warn("Failed to remove post from Elasticsearch: {}", postId, e);
        }

        // Delete the post (cascade will delete related entities, count will be automatically recalculated)
        postRepository.delete(post);
        
        log.info("Post {} deleted successfully by user: {}", postId, userId);
        return true;
    }
}