package com.jerrygram.application.commands.posts;

import com.jerrygram.application.common.BlobContainers;
import com.jerrygram.application.common.HashtagExtractor;
import com.jerrygram.application.common.PostIndex;
import com.jerrygram.application.dtos.PostListItemDto;
import com.jerrygram.application.dtos.SimpleUserDto;
import com.jerrygram.application.interfaces.IBlobService;
import com.jerrygram.application.interfaces.ICacheService;
import com.jerrygram.application.interfaces.ICommandHandler;
import com.jerrygram.application.interfaces.IElasticService;
import com.jerrygram.domain.entities.Post;
import com.jerrygram.domain.entities.PostTag;
import com.jerrygram.domain.entities.Tag;
import com.jerrygram.infrastructure.repositories.PostRepository;
import com.jerrygram.infrastructure.repositories.PostTagRepository;
import com.jerrygram.infrastructure.repositories.TagRepository;
import com.jerrygram.infrastructure.repositories.UserRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;

@Service
@RequiredArgsConstructor
@Slf4j
public class CreatePostCommandHandler implements ICommandHandler<CreatePostCommand, PostListItemDto> {

    private final PostRepository postRepository;
    private final UserRepository userRepository;
    private final PostTagRepository postTagRepository;
    private final TagRepository tagRepository;
    private final IElasticService elasticService;
    private final IBlobService blobService;
    private final ICacheService cacheService;

    @Override
    @Transactional
    public PostListItemDto handle(CreatePostCommand command) {
        var dto = command.getCreatePostDto();
        var authorId = command.getAuthorId();
        
        log.info("Creating post for user: {}", authorId);

        var author = userRepository.findById(authorId)
                .orElseThrow(() -> new IllegalArgumentException("User not found"));

        // Upload image if provided
        String imageUrl = null;
        if (dto.getImage() != null && !dto.getImage().isEmpty()) {
            try {
                imageUrl = blobService.upload(dto.getImage(), BlobContainers.POSTS);
                log.info("Image uploaded successfully: {}", imageUrl);
            } catch (Exception e) {
                log.error("Failed to upload image for post", e);
                throw new RuntimeException("Failed to upload image", e);
            }
        }

        var post = Post.builder()
                .caption(dto.getCaption())
                .imageUrl(imageUrl)
                .visibility(dto.getVisibility())
                .userId(authorId)
                .build();

        postRepository.save(post);
        
        // Extract and save hashtags using PostCaption value object
        var hashtags = post.getHashtags();
        for (String hashtag : hashtags) {
            // Find or create tag
            var tag = tagRepository.findByName(hashtag)
                    .orElseGet(() -> {
                        var newTag = Tag.builder()
                                .name(hashtag)
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
        }
        
        // Index in Elasticsearch
        try {
            elasticService.indexPost(PostIndex.builder()
                    .id(post.getId().toString())
                    .caption(post.getCaption())
                    .imageUrl(post.getImageUrl())
                    .authorId(author.getId().toString())
                    .authorUsername(author.getUsername())
                    .visibility(String.valueOf(post.getVisibility().ordinal()))
                    .likesCount(post.getLikesCount())
                    .commentsCount(post.getCommentsCount())
                    .tags(hashtags)
                    .createdAt(post.getCreatedAt())
                    .isActive(true)
                    .build());
        } catch (Exception e) {
            log.warn("Failed to index post in Elasticsearch: {}", post.getId(), e);
        }
        
        // Invalidate related caches
        invalidateRelatedCaches(post, hashtags);
        
        log.info("Post {} created successfully by user: {} with {} hashtags", 
                post.getId(), authorId, hashtags.size());

        return PostListItemDto.builder()
                .id(post.getId())
                .caption(post.getCaption())
                .imageUrl(post.getImageUrl())
                .createdAt(post.getCreatedAt())
                .likes(post.getLikesCount())
                .liked(false) // New post has no likes
                .user(SimpleUserDto.builder()
                        .id(author.getId())
                        .username(author.getUsername())
                        .profileImageUrl(author.getProfileImageUrl())
                        .build())
                .score(0.0)
                .build();
    }
    
    private void invalidateRelatedCaches(Post post, List<String> hashtags) {
        try {
            // Invalidate public posts cache
            cacheService.deleteByPattern("public_posts_page_*");
            
            // Invalidate user feed cache
            cacheService.delete("user_feed_" + post.getUserId());
            
            // Invalidate hashtag-related autocomplete caches
            for (String hashtag : hashtags) {
                cacheService.delete("autocomplete:#" + hashtag);
                cacheService.delete("autocomplete:" + hashtag);
                
                if (hashtag.length() >= 2) {
                    String prefix = hashtag.substring(0, 2);
                    cacheService.deleteByPattern("autocomplete:#" + prefix + "*");
                    cacheService.deleteByPattern("autocomplete:" + prefix + "*");
                }
            }
            
            log.debug("Cache invalidated for new post: {}", post.getId());
        } catch (Exception e) {
            log.warn("Failed to invalidate cache for post: {}", post.getId(), e);
        }
    }
}