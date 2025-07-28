package com.jerrygram.infrastructure.repositories;

import com.jerrygram.domain.entities.Comment;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

import java.util.UUID;

@Repository
public interface CommentRepository extends JpaRepository<Comment, UUID> {
    
    /**
     * Find comments by post ID
     */
    Page<Comment> findByPostIdOrderByCreatedAtDesc(UUID postId, Pageable pageable);
    
    /**
     * Find comments by author ID
     */
    Page<Comment> findByUserIdOrderByCreatedAtDesc(UUID userId, Pageable pageable);
    
    /**
     * Get comments count by post
     */
    @Query("SELECT COUNT(c) FROM Comment c WHERE c.postId = :postId")
    Long getCommentsCountByPost(@Param("postId") UUID postId);
    
    /**
     * Get comments count by user
     */
    @Query("SELECT COUNT(c) FROM Comment c WHERE c.userId = :userId")
    Long getCommentsCountByUser(@Param("userId") UUID userId);
}