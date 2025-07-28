package com.jerrygram.application.commands.comments;

import com.jerrygram.application.interfaces.ICommandHandler;
import com.jerrygram.infrastructure.repositories.CommentRepository;
import com.jerrygram.infrastructure.repositories.NotificationRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.NoSuchElementException;

@Service
@RequiredArgsConstructor
@Slf4j
public class DeleteCommentCommandHandler implements ICommandHandler<DeleteCommentCommand, Boolean> {

    private final CommentRepository commentRepository;
    private final NotificationRepository notificationRepository;

    @Override
    @Transactional
    public Boolean handle(DeleteCommentCommand command) {
        var commentId = command.getCommentId();
        var userId = command.getUserId();
        
        log.info("Deleting comment: {} by user: {}", commentId, userId);

        var comment = commentRepository.findById(commentId)
                .orElseThrow(() -> new NoSuchElementException("Comment not found."));

        // Authorization check - only comment author can delete
        if (!comment.getUserId().equals(userId)) {
            throw new SecurityException("You are not the owner of this comment.");
        }
        
        // Delete comment
        commentRepository.delete(comment);
        
        // Delete related notification if exists
        try {
            notificationRepository.deleteCommentNotification(comment.getPostId(), userId);
            log.debug("Deleted comment notification for post {} by user {}", comment.getPostId(), userId);
        } catch (Exception e) {
            log.warn("Failed to delete comment notification: {}", e.getMessage());
        }
        
        log.info("Comment {} deleted successfully", commentId);
        return true;
    }
}