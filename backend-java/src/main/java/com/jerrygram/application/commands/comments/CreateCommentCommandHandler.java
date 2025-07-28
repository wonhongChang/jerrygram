package com.jerrygram.application.commands.comments;

import com.jerrygram.application.dtos.CommentDto;
import com.jerrygram.application.dtos.SimpleUserDto;
import com.jerrygram.application.interfaces.ICommandHandler;
import com.jerrygram.domain.entities.Comment;
import com.jerrygram.domain.entities.Notification;
import com.jerrygram.domain.enums.NotificationType;
import com.jerrygram.infrastructure.repositories.CommentRepository;
import com.jerrygram.infrastructure.repositories.NotificationRepository;
import com.jerrygram.infrastructure.repositories.PostRepository;
import com.jerrygram.infrastructure.repositories.UserRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.NoSuchElementException;

@Service
@RequiredArgsConstructor
@Slf4j
public class CreateCommentCommandHandler implements ICommandHandler<CreateCommentCommand, CommentDto> {

    private final CommentRepository commentRepository;
    private final PostRepository postRepository;
    private final UserRepository userRepository;
    private final NotificationRepository notificationRepository;

    @Override
    @Transactional
    public CommentDto handle(CreateCommentCommand command) {
        var postId = command.getPostId();
        var authorId = command.getAuthorId();
        var content = command.getContent();
        
        log.info("Creating comment on post: {} by user: {}", postId, authorId);

        var post = postRepository.findById(postId)
                .orElseThrow(() -> new NoSuchElementException("Post not found."));
        var author = userRepository.findById(authorId)
                .orElseThrow(() -> new SecurityException("User not found."));

        var comment = Comment.builder()
                .content(content)
                .postId(postId)
                .userId(authorId)
                .build();

        commentRepository.save(comment);
        
        // Save notification if commenter is not the post owner
        if (!post.getUser().getId().equals(authorId)) {
            var notification = Notification.builder()
                    .recipientId(post.getUser().getId())
                    .fromUserId(authorId)
                    .type(NotificationType.Comment)
                    .postId(postId)
                    .message(author.getUsername() + " commented on your post.")
                    .build();
            
            notificationRepository.save(notification);
        }
        
        log.info("Comment {} created successfully", comment.getId());

        return CommentDto.builder()
                .id(comment.getId())
                .content(comment.getContent())
                .createdAt(comment.getCreatedAt())
                .user(SimpleUserDto.builder()
                        .id(author.getId())
                        .username(author.getUsername())
                        .profileImageUrl(author.getProfileImageUrl())
                        .build())
                .build();
    }
}