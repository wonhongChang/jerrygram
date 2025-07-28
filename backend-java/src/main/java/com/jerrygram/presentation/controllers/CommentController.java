package com.jerrygram.presentation.controllers;

import com.jerrygram.application.commands.comments.CreateCommentCommand;
import com.jerrygram.application.commands.comments.DeleteCommentCommand;
import com.jerrygram.application.dtos.CommentDto;
import com.jerrygram.application.interfaces.ICommandHandler;
import com.jerrygram.application.interfaces.IQueryHandler;
import com.jerrygram.application.queries.comments.GetPostCommentsQuery;
import com.jerrygram.domain.entities.User;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.data.domain.Page;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.*;

import java.util.Map;
import java.util.UUID;

@RestController
@RequestMapping("/api/comments")
@RequiredArgsConstructor
@Slf4j
public class CommentController {

    private final ICommandHandler<CreateCommentCommand, CommentDto> createCommentCommandHandler;
    private final ICommandHandler<DeleteCommentCommand, Boolean> deleteCommentCommandHandler;
    private final IQueryHandler<GetPostCommentsQuery, Page<CommentDto>> getPostCommentsQueryHandler;

    @PostMapping
    public ResponseEntity<CommentDto> createComment(
            @RequestBody Map<String, Object> request,
            Authentication authentication) {
        
        User currentUser = (User) authentication.getPrincipal();
        var postId = UUID.fromString((String) request.get("postId"));
        var content = (String) request.get("content");
        
        log.info("Creating comment on post: {} by user: {}", postId, currentUser.getUsername());
        
        var command = new CreateCommentCommand(postId, currentUser.getId(), content);
        var result = createCommentCommandHandler.handle(command);
        
        log.info("Comment created successfully: {}", result.getId());
        return ResponseEntity.ok(result);
    }

    @GetMapping("/post/{postId}")
    public ResponseEntity<Page<CommentDto>> getPostComments(
            @PathVariable UUID postId,
            @RequestParam(defaultValue = "0") int page,
            @RequestParam(defaultValue = "10") int size) {
        
        log.info("Getting comments for post: {}, page: {}, size: {}", postId, page, size);
        
        var query = new GetPostCommentsQuery(postId, page, size);
        var result = getPostCommentsQueryHandler.handle(query);
        
        log.info("Retrieved {} comments for post", result.getNumberOfElements());
        return ResponseEntity.ok(result);
    }

    @DeleteMapping("/{commentId}")
    public ResponseEntity<Boolean> deleteComment(
            @PathVariable UUID commentId,
            Authentication authentication) {
        
        User currentUser = (User) authentication.getPrincipal();
        log.info("User {} deleting comment: {}", currentUser.getUsername(), commentId);
        
        var command = new DeleteCommentCommand(commentId, currentUser.getId());
        var result = deleteCommentCommandHandler.handle(command);
        
        log.info("Comment {} deleted successfully", commentId);
        return ResponseEntity.ok(result);
    }
}