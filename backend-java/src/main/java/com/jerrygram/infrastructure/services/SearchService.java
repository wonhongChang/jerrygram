package com.jerrygram.infrastructure.services;

import com.jerrygram.application.dtos.PostDto;
import com.jerrygram.application.dtos.SearchResultDto;
import com.jerrygram.application.dtos.UserProfileDto;
import com.jerrygram.application.interfaces.ICacheService;
import com.jerrygram.application.interfaces.ISearchService;
import com.jerrygram.domain.entities.Post;
import com.jerrygram.domain.entities.User;
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
import java.util.List;
import java.util.UUID;

@Service
@RequiredArgsConstructor
@Slf4j
public class SearchService implements ISearchService {

    private final UserRepository userRepository;
    private final PostRepository postRepository;
    private final UserFollowRepository userFollowRepository;
    private final PostTagRepository postTagRepository;
    private final ICacheService cacheService;

    @Override
    public SearchResultDto search(String query, String userId) {
        log.info("Searching for: {} by user: {}", query, userId);

        UUID parsedUserId = parseUserId(userId);
        List<UUID> followingIds = getFollowingIds(parsedUserId);
        
        if (query.startsWith("#")) {
            return searchByHashtag(query, parsedUserId, followingIds);
        } else {
            return searchGeneral(query, parsedUserId, followingIds);
        }
    }
    
    private UUID parseUserId(String userIdStr) {
        if (userIdStr == null || userIdStr.isEmpty()) {
            return null;
        }
        try {
            return UUID.fromString(userIdStr);
        } catch (IllegalArgumentException e) {
            log.warn("Invalid userId format: {}", userIdStr);
            return null;
        }
    }
    
    private List<UUID> getFollowingIds(UUID userId) {
        if (userId == null) {
            return List.of();
        }
        return userFollowRepository.getFollowingIds(userId);
    }
    
    private SearchResultDto searchByHashtag(String query, UUID userId, List<UUID> followingIds) {
        String tag = query.substring(1).toLowerCase();
        var postIds = postTagRepository.getPostIdsByTag(tag);
        var posts = postRepository.findByIdIn(postIds);
        
        var filteredPosts = posts.stream()
                .filter(post -> isPostVisible(post, userId, followingIds))
                .map(this::mapToPostDto)
                .toList();
        
        return SearchResultDto.builder()
                .users(List.of())
                .posts(filteredPosts)
                .hashtags(List.of(tag))
                .build();
    }
    
    private SearchResultDto searchGeneral(String query, UUID userId, List<UUID> followingIds) {
        var pageable = PageRequest.of(0, 10);
        
        var posts = postRepository.findByCaptionContainingIgnoreCase(query, pageable)
                .stream()
                .filter(post -> isPostVisible(post, userId, followingIds))
                .map(this::mapToPostDto)
                .toList();
        
        var users = userRepository.findByUsernameContainingIgnoreCase(query, pageable)
                .stream()
                .map(this::mapToUserDto)
                .toList();
        
        return SearchResultDto.builder()
                .users(users)
                .posts(posts)
                .hashtags(List.of())
                .build();
    }
    
    private boolean isPostVisible(Post post, UUID userId, List<UUID> followingIds) {
        return post.getVisibility() == PostVisibility.Public ||
               (post.getVisibility() == PostVisibility.FollowersOnly && 
                userId != null && followingIds.contains(post.getUser().getId()));
    }
    
    private PostDto mapToPostDto(Post post) {
        return PostDto.builder()
                .id(post.getId())
                .caption(post.getCaption())
                .imageUrl(post.getImageUrl())
                .visibility(post.getVisibility())
                .author(mapToUserDto(post.getUser()))
                .likesCount(post.getLikesCount())
                .commentsCount(post.getCommentsCount())
                .createdAt(post.getCreatedAt())
                .build();
    }
    
    private UserProfileDto mapToUserDto(User user) {
        return UserProfileDto.builder()
                .id(user.getId())
                .username(user.getUsername())
                .email(user.getEmail())
                .profileImageUrl(user.getProfileImageUrl())
                .createdAt(user.getCreatedAt())
                .followers(0)
                .followings(0)
                .build();
    }

    @Override
    public SearchResultDto autocomplete(String query) {
        log.info("Autocomplete for: {}", query);
        
        String cacheKey = "autocomplete:" + query.trim().toLowerCase();
        
        // Try cache first
        var cached = tryGetFromCache(cacheKey);
        if (cached != null) {
            log.debug("Autocomplete cache hit for query: {}", query);
            return cached;
        }
        
        log.debug("Autocomplete cache miss for query: {}", query);
        
        SearchResultDto result = performAutocomplete(query.trim());
        
        // Cache for 5 minutes
        trySetCache(cacheKey, result);
        
        return result;
    }
    
    private SearchResultDto tryGetFromCache(String cacheKey) {
        try {
            var cached = cacheService.get(cacheKey, SearchResultDto.class);
            return cached.orElse(null);
        } catch (Exception e) {
            log.warn("Cache get failed for key: {}", cacheKey, e);
            return null;
        }
    }
    
    private void trySetCache(String cacheKey, SearchResultDto result) {
        try {
            cacheService.set(cacheKey, result, Duration.ofMinutes(5));
            log.info("Autocomplete result cached");
        } catch (Exception e) {
            log.warn("Cache set failed for key: {}", cacheKey, e);
        }
    }
    
    private SearchResultDto performAutocomplete(String query) {
        if (query.startsWith("#")) {
            return autocompleteHashtag(query.substring(1).toLowerCase());
        } else {
            return autocompleteGeneral(query);
        }
    }
    
    private SearchResultDto autocompleteHashtag(String keyword) {
        var hashtags = postTagRepository.findHashtagsByPrefix(keyword)
                .stream()
                .limit(3)
                .toList();
        
        var users = userRepository.findByUsernameStartingWithIgnoreCase(keyword, PageRequest.of(0, 10))
                .stream()
                .map(this::mapToUserDto)
                .toList();
        
        return SearchResultDto.builder()
                .users(users)
                .posts(List.of())
                .hashtags(hashtags)
                .build();
    }
    
    private SearchResultDto autocompleteGeneral(String query) {
        var hashtags = postTagRepository.findHashtagsByPrefix(query.toLowerCase())
                .stream()
                .limit(3)
                .toList();
        
        var users = userRepository.findByUsernameStartingWithIgnoreCase(query, PageRequest.of(0, 10))
                .stream()
                .map(this::mapToUserDto)
                .toList();
        
        return SearchResultDto.builder()
                .users(users)
                .posts(List.of())
                .hashtags(hashtags)
                .build();
    }
}