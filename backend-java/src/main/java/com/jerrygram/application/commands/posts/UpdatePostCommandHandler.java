package com.jerrygram.application.commands.posts;

import com.jerrygram.application.common.BlobContainers;
import com.jerrygram.application.common.PostIndex;
import com.jerrygram.application.common.TagIndex;
import com.jerrygram.domain.entities.Post;
import com.jerrygram.application.interfaces.IBlobService;
import com.jerrygram.application.interfaces.ICommandHandler;
import com.jerrygram.application.interfaces.IElasticService;
import com.jerrygram.domain.entities.PostTag;
import com.jerrygram.domain.entities.Tag;
import com.jerrygram.infrastructure.repositories.PostRepository;
import com.jerrygram.infrastructure.repositories.PostTagRepository;
import com.jerrygram.infrastructure.repositories.TagRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.HashSet;
import java.util.List;
import java.util.NoSuchElementException;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
@Slf4j
public class UpdatePostCommandHandler implements ICommandHandler<UpdatePostCommand, Post> {

    private final PostRepository postRepository;
    private final PostTagRepository postTagRepository;
    private final TagRepository tagRepository;
    private final IBlobService blobService;
    private final IElasticService elasticService;

    @Override
    @Transactional
    public Post handle(UpdatePostCommand command) {
        var postId = command.getPostId();
        var userId = command.getUserId();
        var dto = command.getUpdatePostDto();
        
        log.info("Updating post: {} by user: {}", postId, userId);

        var post = postRepository.findById(postId)
                .orElseThrow(() -> new NoSuchElementException("Post not found."));

        // Authorization check - only post owner can update
        if (!post.getUser().getId().equals(userId)) {
            throw new SecurityException("You are not the owner of this post.");
        }

        // Get current hashtags before updating
        var oldHashtags = post.getHashtags();

        boolean captionChanged = false;
        
        // Update caption if provided
        if (dto.getCaption() != null && !dto.getCaption().trim().isEmpty() && !dto.getCaption().equals(post.getCaption())) {
            post.setCaption(dto.getCaption());
            captionChanged = true;
        }

        // Update visibility if provided
        if (dto.getVisibility() != null) {
            post.setVisibility(dto.getVisibility());
        }

        // Update image if provided
        if (dto.getImage() != null && dto.getImage().getSize() > 0) {
            // Delete old image if exists
            if (post.getImageUrl() != null && !post.getImageUrl().isEmpty()) {
                blobService.delete(post.getImageUrl(), BlobContainers.POSTS);
            }
            
            // Upload new image
            String newImageUrl = blobService.upload(dto.getImage(), BlobContainers.POSTS);
            post.setImageUrl(newImageUrl);
        }

        // Save post first
        postRepository.save(post);

        // Update hashtags if caption changed
        if (captionChanged) {
            updateHashtags(post, oldHashtags);
        }

        // Update Elasticsearch index
        try {
            elasticService.indexPost(PostIndex.builder()
                    .id(post.getId().toString())
                    .caption(post.getCaption())
                    .imageUrl(post.getImageUrl())
                    .authorId(post.getUser().getId().toString())
                    .authorUsername(post.getUser().getUsername())
                    .visibility(post.getVisibility().toString())
                    .likesCount(post.getLikesCount())
                    .commentsCount(post.getCommentsCount())
                    .tags(post.getHashtags())
                    .createdAt(post.getCreatedAt())
                    .isActive(true)
                    .build());
        } catch (Exception e) {
            log.warn("Failed to update post in Elasticsearch: {}", postId, e);
        }
        
        log.info("Post {} updated successfully", postId);

        return post;
    }

    private void updateHashtags(Post post, List<String> oldHashtags) {
        var newHashtags = post.getHashtags();
        var newHashtagSet = new HashSet<>(newHashtags);
        var oldHashtagSet = new HashSet<>(oldHashtags);

        // Remove old hashtag relationships that are no longer present
        for (String oldHashtag : oldHashtags) {
            if (!newHashtagSet.contains(oldHashtag)) {
                var tag = tagRepository.findByName(oldHashtag);
                if (tag.isPresent()) {
                    var postTagToRemove = postTagRepository.findByPostIdAndTagId(post.getId(), tag.get().getId());
                    postTagToRemove.ifPresent(postTagRepository::delete);
                }
            }
        }

        // Add new hashtag relationships
        for (String newHashtag : newHashtags) {
            if (!oldHashtagSet.contains(newHashtag)) {
                // Find or create tag
                var tag = tagRepository.findByName(newHashtag)
                        .orElseGet(() -> {
                            var newTag = Tag.builder()
                                    .name(newHashtag)
                                    .build();
                            return tagRepository.save(newTag);
                        });
                
                // Create PostTag relationship
                var postTag = PostTag.builder()
                        .postId(post.getId())
                        .tagId(tag.getId())
                        .post(post)
                        .tag(tag)
                        .build();
                postTagRepository.save(postTag);

                // Index tag in Elasticsearch
                try {
                    elasticService.indexTag(TagIndex.builder()
                            .id(tag.getId().toString())
                            .name(tag.getName())
                            .build());
                } catch (Exception e) {
                    log.warn("Failed to index tag in Elasticsearch: {}", newHashtag, e);
                }
            }
        }
    }
}