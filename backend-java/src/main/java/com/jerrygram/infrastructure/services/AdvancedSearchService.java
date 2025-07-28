package com.jerrygram.infrastructure.services;

import com.jerrygram.application.common.HashtagExtractor;
import com.jerrygram.application.dtos.PostDto;
import com.jerrygram.application.dtos.SearchResultDto;
import com.jerrygram.application.dtos.UserProfileDto;
import com.jerrygram.application.interfaces.ICacheService;
import com.jerrygram.application.interfaces.IElasticService;
import com.jerrygram.application.interfaces.ISearchService;
import com.jerrygram.domain.enums.PostVisibility;
import com.jerrygram.infrastructure.repositories.PostRepository;
import com.jerrygram.infrastructure.repositories.PostTagRepository;
import com.jerrygram.infrastructure.repositories.UserFollowRepository;
import com.jerrygram.infrastructure.repositories.UserRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.data.domain.PageRequest;
import org.springframework.stereotype.Service;

import java.time.Duration;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

@Service("advancedSearchService")
@RequiredArgsConstructor
@Slf4j
public class AdvancedSearchService implements ISearchService {

    private final UserRepository userRepository;
    private final PostRepository postRepository;
    private final PostTagRepository postTagRepository;
    private final UserFollowRepository userFollowRepository;
    private final IElasticService elasticService;
    private final ICacheService cacheService;

    @Override
    public SearchResultDto search(String query, String userId) {
        log.info("Advanced searching for: {} by user: {}", query, userId);
        
        UUID userIdParsed = null;
        List<UUID> followingIds = new ArrayList<>();
        
        if (userId != null && !userId.isEmpty()) {
            try {
                userIdParsed = UUID.fromString(userId);
                followingIds = userFollowRepository.getFollowingUserIds(userIdParsed);
            } catch (Exception e) {
                log.warn("Invalid userId format: {}", userId);
            }
        }
        
        // Handle hashtag search
        if (query.startsWith("#")) {
            return searchByHashtag(query.substring(1), followingIds);
        }
        
        // Try Elasticsearch first, fallback to database
        try {
            return searchWithElasticsearch(query, followingIds);
        } catch (Exception e) {
            log.warn("Elasticsearch search failed, using database fallback", e);
            return searchWithDatabase(query);
        }
    }
    
    public SearchResultDto autocomplete(String query) {
        String cacheKey = "autocomplete:" + query.toLowerCase();
        
        try {
            var cached = cacheService.get(cacheKey, SearchResultDto.class);
            if (cached.isPresent()) {
                log.debug("Autocomplete cache hit for: {}", query);
                return cached.get();
            }
        } catch (Exception e) {
            log.warn("Cache get failed for autocomplete: {}", query, e);
        }
        
        SearchResultDto result;
        
        if (query.startsWith("#")) {
            // Hashtag autocomplete
            var hashtags = postTagRepository.findTagNamesByPrefix(query.substring(1))
                    .stream()
                    .limit(10)
                    .map(tag -> "#" + tag)
                    .toList();
            
            result = SearchResultDto.builder()
                    .users(List.of())
                    .posts(List.of())
                    .hashtags(hashtags)
                    .build();
        } else {
            // User autocomplete
            var pageable = PageRequest.of(0, 10);
            var users = userRepository.findByUsernameStartingWithIgnoreCase(query, pageable)
                    .stream()
                    .map(user -> UserProfileDto.builder()
                            .id(user.getId())
                            .username(user.getUsername())
                            .email(user.getEmail())
                            .profileImageUrl(user.getProfileImageUrl())
                            .createdAt(user.getCreatedAt())
                            .followers(user.getFollowersCount())
                            .followings(user.getFollowingCount())
                            .build())
                    .toList();
            
            result = SearchResultDto.builder()
                    .users(users)
                    .posts(List.of())
                    .hashtags(List.of())
                    .build();
        }
        
        // Cache the result
        try {
            cacheService.set(cacheKey, result, Duration.ofMinutes(5));
            log.debug("Cached autocomplete result for: {}", query);
        } catch (Exception e) {
            log.warn("Cache set failed for autocomplete: {}", query, e);
        }
        
        return result;
    }
    
    private SearchResultDto searchByHashtag(String tag, List<UUID> followingIds) {
        var normalizedTag = HashtagExtractor.normalizeHashtag(tag);
        var postIds = postTagRepository.findPostIdsByTagName(normalizedTag);
        
        var posts = postRepository.findAllById(postIds)
                .stream()
                .filter(post -> post.getVisibility() == PostVisibility.Public || 
                               followingIds.contains(post.getUser().getId()))
                .map(post -> PostDto.builder()
                        .id(post.getId())
                        .caption(post.getCaption())
                        .imageUrl(post.getImageUrl())
                        .visibility(post.getVisibility())
                        .author(UserProfileDto.builder()
                                .id(post.getUser().getId())
                                .username(post.getUser().getUsername())
                                .email(post.getUser().getEmail())
                                .profileImageUrl(post.getUser().getProfileImageUrl())
                                .createdAt(post.getUser().getCreatedAt())
                                .followers(post.getUser().getFollowersCount())
                                .followings(post.getUser().getFollowingCount())
                                .build())
                        .likesCount(post.getLikesCount())
                        .commentsCount(post.getCommentsCount())
                        .createdAt(post.getCreatedAt())
                        .build())
                .toList();
        
        return SearchResultDto.builder()
                .users(List.of())
                .posts(posts)
                .hashtags(List.of("#" + normalizedTag))
                .build();
    }
    
    private SearchResultDto searchWithElasticsearch(String query, List<UUID> followingIds) {
        var userIndices = elasticService.searchUsers(query, 10);
        var postIndices = elasticService.searchPosts(query);
        var tagIndices = elasticService.searchTags(query, 5);
        
        var users = userIndices.stream()
                .map(userIndex -> UserProfileDto.builder()
                        .id(UUID.fromString(userIndex.getId()))
                        .username(userIndex.getUsername())
                        .email(userIndex.getEmail())
                        .profileImageUrl(userIndex.getProfileImageUrl())
                        .followers(userIndex.getFollowersCount())
                        .followings(userIndex.getFollowingCount())
                        .build())
                .toList();
        
        var posts = postIndices.stream()
                .filter(postIndex -> "Public".equals(postIndex.getVisibility()) || 
                                   followingIds.contains(UUID.fromString(postIndex.getAuthorId())))
                .map(postIndex -> PostDto.builder()
                        .id(UUID.fromString(postIndex.getId()))
                        .caption(postIndex.getCaption())
                        .imageUrl(postIndex.getImageUrl())
                        .visibility(com.jerrygram.domain.enums.PostVisibility.valueOf(postIndex.getVisibility()))
                        .author(UserProfileDto.builder()
                                .id(UUID.fromString(postIndex.getAuthorId()))
                                .username(postIndex.getAuthorUsername())
                                .build())
                        .likesCount(postIndex.getLikesCount())
                        .commentsCount(postIndex.getCommentsCount())
                        .createdAt(postIndex.getCreatedAt())
                        .build())
                .toList();
        
        var hashtags = tagIndices.stream()
                .map(tagIndex -> "#" + tagIndex.getName())
                .toList();
        
        return SearchResultDto.builder()
                .users(users)
                .posts(posts)
                .hashtags(hashtags)
                .build();
    }
    
    private SearchResultDto searchWithDatabase(String query) {
        var pageable = PageRequest.of(0, 10);
        
        var users = userRepository.findByUsernameContainingIgnoreCase(query, pageable)
                .stream()
                .map(user -> UserProfileDto.builder()
                        .id(user.getId())
                        .username(user.getUsername())
                        .email(user.getEmail())
                        .profileImageUrl(user.getProfileImageUrl())
                        .createdAt(user.getCreatedAt())
                        .followers(user.getFollowersCount())
                        .followings(user.getFollowingCount())
                        .build())
                .toList();
        
        var posts = postRepository.findByCaptionContainingIgnoreCase(query, pageable)
                .stream()
                .map(post -> PostDto.builder()
                        .id(post.getId())
                        .caption(post.getCaption())
                        .imageUrl(post.getImageUrl())
                        .visibility(post.getVisibility())
                        .author(UserProfileDto.builder()
                                .id(post.getUser().getId())
                                .username(post.getUser().getUsername())
                                .email(post.getUser().getEmail())
                                .profileImageUrl(post.getUser().getProfileImageUrl())
                                .createdAt(post.getUser().getCreatedAt())
                                .followers(post.getUser().getFollowersCount())
                                .followings(post.getUser().getFollowingCount())
                                .build())
                        .likesCount(post.getLikesCount())
                        .commentsCount(post.getCommentsCount())
                        .createdAt(post.getCreatedAt())
                        .build())
                .toList();
        
        var hashtags = postTagRepository.findTagNamesByPrefix(query)
                .stream()
                .limit(5)
                .map(tag -> "#" + tag)
                .toList();
        
        return SearchResultDto.builder()
                .users(users)
                .posts(posts)
                .hashtags(hashtags)
                .build();
    }
}