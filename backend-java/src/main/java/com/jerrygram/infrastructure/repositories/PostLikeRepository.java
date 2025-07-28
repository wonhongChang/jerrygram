package com.jerrygram.infrastructure.repositories;

import com.jerrygram.application.dtos.PagedResult;
import com.jerrygram.application.dtos.SimpleUserDto;
import com.jerrygram.domain.entities.PostLike;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

@Repository
public interface PostLikeRepository extends JpaRepository<PostLike, UUID> {
    
    /**
     * Find like by post and user
     */
    Optional<PostLike> findByPostIdAndUserId(UUID postId, UUID userId);
    
    /**
     * Check if user liked post
     */
    boolean existsByPostIdAndUserId(UUID postId, UUID userId);
    
    /**
     * Find likes by post ID
     */
    Page<PostLike> findByPostIdOrderByCreatedAtDesc(UUID postId, Pageable pageable);
    
    /**
     * Find likes by user ID
     */
    Page<PostLike> findByUserIdOrderByCreatedAtDesc(UUID userId, Pageable pageable);
    
    /**
     * Get likes count by post
     */
    @Query("SELECT COUNT(pl) FROM PostLike pl WHERE pl.post.id = :postId")
    Long getLikesCountByPost(@Param("postId") UUID postId);
    
    /**
     * Delete like by post and user
     */
    void deleteByPostIdAndUserId(UUID postId, UUID userId);
    
    /**
     * Find likes by post ID (for likes listing)
     */
    Page<PostLike> findByPostId(UUID postId, Pageable pageable);
    
    /**
     * Get post likes users with pagination (matching .NET GetPostLikesAsync)
     */
    @Query("SELECT new com.jerrygram.application.dtos.SimpleUserDto(u.id, u.username, u.profileImageUrl) " +
           "FROM PostLike pl JOIN pl.user u WHERE pl.post.id = :postId " +
           "ORDER BY pl.createdAt DESC")
    List<SimpleUserDto> getPostLikesUsers(@Param("postId") UUID postId, Pageable pageable);
    
    /**
     * Count post likes
     */
    @Query("SELECT COUNT(pl) FROM PostLike pl WHERE pl.post.id = :postId")
    long countByPostId(@Param("postId") UUID postId);
}