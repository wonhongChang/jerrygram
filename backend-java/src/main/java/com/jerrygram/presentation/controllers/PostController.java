package com.jerrygram.presentation.controllers;

import com.jerrygram.application.commands.posts.CreatePostCommand;
import com.jerrygram.application.commands.posts.DeletePostCommand;
import com.jerrygram.application.commands.posts.LikePostCommand;
import com.jerrygram.application.commands.posts.UpdatePostCommand;
import com.jerrygram.application.dtos.CreatePostDto;
import com.jerrygram.application.dtos.PostListItemDto;
import com.jerrygram.application.dtos.PagedResult;
import com.jerrygram.application.dtos.SimpleUserDto;
import com.jerrygram.application.dtos.UpdatePostDto;
import com.jerrygram.domain.enums.PostVisibility;
import org.springframework.web.multipart.MultipartFile;
import com.jerrygram.domain.entities.Post;
import com.jerrygram.application.interfaces.ICommandHandler;
import com.jerrygram.application.interfaces.IQueryHandler;
import com.jerrygram.application.queries.posts.GetPostByIdQuery;
import com.jerrygram.application.queries.posts.GetPostLikesQuery;
import com.jerrygram.application.queries.posts.GetPublicPostsQuery;
import com.jerrygram.application.queries.posts.GetTimelineQuery;
import com.jerrygram.domain.entities.User;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.*;

import java.util.UUID;

@RestController
@RequestMapping("/api/posts")
@RequiredArgsConstructor
@Slf4j
public class PostController {

    private final ICommandHandler<CreatePostCommand, PostListItemDto> createPostCommandHandler;
    private final ICommandHandler<UpdatePostCommand, Post> updatePostCommandHandler;
    private final ICommandHandler<DeletePostCommand, Boolean> deletePostCommandHandler;
    private final ICommandHandler<LikePostCommand, Boolean> likePostCommandHandler;
    private final IQueryHandler<GetPostByIdQuery, PostListItemDto> getPostByIdQueryHandler;
    private final IQueryHandler<GetPostLikesQuery, PagedResult<SimpleUserDto>> getPostLikesQueryHandler;
    private final IQueryHandler<GetPublicPostsQuery, PagedResult<PostListItemDto>> getPublicPostsQueryHandler;
    private final IQueryHandler<GetTimelineQuery, PagedResult<PostListItemDto>> getTimelineQueryHandler;

    @PostMapping(consumes = "multipart/form-data")
    public ResponseEntity<PostListItemDto> createPost(
            @RequestParam(value = "caption", required = false) String caption,
            @RequestParam(value = "image", required = false) MultipartFile image,
            @RequestParam(value = "visibility", defaultValue = "0") int visibilityValue,
            Authentication authentication) {
        
        User currentUser = (User) authentication.getPrincipal();
        log.info("Creating post for user: {}", currentUser.getUsername());
        
        // Convert int to PostVisibility enum
        PostVisibility visibility = PostVisibility.values()[visibilityValue];
        
        // Create DTO from form parameters
        var createPostDto = new CreatePostDto(caption, image, visibility);
        
        var command = new CreatePostCommand(createPostDto, currentUser.getId());
        var result = createPostCommandHandler.handle(command);
        
        log.info("Post created successfully: {}", result.getId());
        return ResponseEntity.ok(result);
    }

    @PutMapping("/{postId}")
    public ResponseEntity<Post> updatePost(
            @PathVariable UUID postId,
            @RequestBody UpdatePostDto updatePostDto,
            Authentication authentication) {
        
        User currentUser = (User) authentication.getPrincipal();
        log.info("User {} updating post: {}", currentUser.getUsername(), postId);
        
        var command = new UpdatePostCommand(postId, currentUser.getId(), updatePostDto);
        var result = updatePostCommandHandler.handle(command);
        
        log.info("Post {} updated successfully", postId);
        return ResponseEntity.ok(result);
    }

    @PostMapping("/{postId}/like")
    public ResponseEntity<Boolean> likePost(
            @PathVariable UUID postId,
            Authentication authentication) {
        
        User currentUser = (User) authentication.getPrincipal();
        log.info("User {} toggling like for post: {}", currentUser.getUsername(), postId);
        
        var command = new LikePostCommand(postId, currentUser.getId());
        var isLiked = likePostCommandHandler.handle(command);
        
        log.info("Post {} like toggled: {}", postId, isLiked);
        return ResponseEntity.ok(isLiked);
    }

    @GetMapping
    public ResponseEntity<PagedResult<PostListItemDto>> getAllPosts(
            @RequestParam(defaultValue = "0") int page,
            @RequestParam(defaultValue = "10") int size,
            Authentication authentication) {
        
        User currentUser = authentication != null ? (User) authentication.getPrincipal() : null;
        var userId = currentUser != null ? currentUser.getId() : null;
        
        log.info("Getting public posts for user: {}, page: {}, size: {}", userId, page, size);
        
        var query = new GetPublicPostsQuery(userId, page, size);
        var result = getPublicPostsQueryHandler.handle(query);
        
        log.info("Retrieved {} public posts", result.getItems().size());
        return ResponseEntity.ok(result);
    }

    @GetMapping("/{postId}")
    public ResponseEntity<PostListItemDto> getPostById(
            @PathVariable UUID postId,
            Authentication authentication) {
        
        User currentUser = authentication != null ? (User) authentication.getPrincipal() : null;
        var userId = currentUser != null ? currentUser.getId() : null;
        
        log.info("Getting post: {} for user: {}", postId, userId);
        
        var query = new GetPostByIdQuery(postId, userId);
        var result = getPostByIdQueryHandler.handle(query);
        
        return ResponseEntity.ok(result);
    }

    @GetMapping("/{postId}/likes")
    public ResponseEntity<PagedResult<SimpleUserDto>> getPostLikes(
            @PathVariable UUID postId,
            @RequestParam(defaultValue = "0") int page,
            @RequestParam(defaultValue = "10") int size) {
        
        log.info("Getting likes for post: {}, page: {}, size: {}", postId, page, size);
        
        var query = new GetPostLikesQuery(postId, page, size);
        var result = getPostLikesQueryHandler.handle(query);
        
        log.info("Retrieved {} likes for post {}", result.getItems().size(), postId);
        return ResponseEntity.ok(result);
    }

    @GetMapping("/feed")
    public ResponseEntity<PagedResult<PostListItemDto>> getUserFeed(
            @RequestParam(defaultValue = "0") int page,
            @RequestParam(defaultValue = "10") int size,
            Authentication authentication) {
        
        User currentUser = (User) authentication.getPrincipal();
        log.info("Getting feed for user: {}, page: {}, size: {}", 
                currentUser.getUsername(), page, size);
        
        var query = new GetTimelineQuery(currentUser.getId(), page, size);
        var result = getTimelineQueryHandler.handle(query);
        
        log.info("Retrieved {} posts for user feed", result.getItems().size());
        return ResponseEntity.ok(result);
    }

    @DeleteMapping("/{postId}")
    public ResponseEntity<Boolean> deletePost(
            @PathVariable UUID postId,
            Authentication authentication) {
        
        User currentUser = (User) authentication.getPrincipal();
        log.info("User {} deleting post: {}", currentUser.getUsername(), postId);
        
        var command = new DeletePostCommand(postId, currentUser.getId());
        var result = deletePostCommandHandler.handle(command);
        
        log.info("Post {} deleted successfully", postId);
        return ResponseEntity.ok(result);
    }
}