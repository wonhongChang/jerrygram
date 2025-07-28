package com.jerrygram.infrastructure.repositories;

import com.jerrygram.domain.entities.UserFollow;
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
public interface UserFollowRepository extends JpaRepository<UserFollow, UUID> {
    
    /**
     * Find follow relationship
     */
    Optional<UserFollow> findByFollowerIdAndFollowingId(UUID followerId, UUID followingId);
    
    /**
     * Check if user follows another user
     */
    boolean existsByFollowerIdAndFollowingId(UUID followerId, UUID followingId);
    
    /**
     * Find users that a user is following
     */
    @Query("SELECT uf.following FROM UserFollow uf WHERE uf.follower.id = :userId")
    Page<com.jerrygram.domain.entities.User> findFollowing(@Param("userId") UUID userId, Pageable pageable);
    
    /**
     * Find users that follow a user
     */
    @Query("SELECT uf.follower FROM UserFollow uf WHERE uf.following.id = :userId")
    Page<com.jerrygram.domain.entities.User> findFollowers(@Param("userId") UUID userId, Pageable pageable);
    
    /**
     * Get following count
     */
    @Query("SELECT COUNT(uf) FROM UserFollow uf WHERE uf.follower.id = :userId")
    Long getFollowingCount(@Param("userId") UUID userId);
    
    /**
     * Get followers count
     */
    @Query("SELECT COUNT(uf) FROM UserFollow uf WHERE uf.following.id = :userId")
    Long getFollowersCount(@Param("userId") UUID userId);
    
    /**
     * Get following user IDs for timeline
     */
    @Query("SELECT uf.following.id FROM UserFollow uf WHERE uf.follower.id = :userId")
    List<UUID> getFollowingUserIds(@Param("userId") UUID userId);
    
    /**
     * Get following IDs (matching .NET GetFollowingIdsAsync)
     */
    @Query("SELECT uf.following.id FROM UserFollow uf WHERE uf.follower.id = :userId")
    List<UUID> getFollowingIds(@Param("userId") UUID userId);
    
    /**
     * Delete follow relationship
     */
    void deleteByFollowerIdAndFollowingId(UUID followerId, UUID followingId);
    
    /**
     * Find follow relationships by follower ID (for getting following list)
     */
    Page<UserFollow> findByFollowerId(UUID followerId, Pageable pageable);
    
    /**
     * Find follow relationships by followed ID (for getting followers list)
     */
    Page<UserFollow> findByFollowingId(UUID followingId, Pageable pageable);
    
    /**
     * Get followers (matching .NET GetFollowersAsync)
     */
    @Query("SELECT new com.jerrygram.application.dtos.SimpleUserDto(u.id, u.username, u.profileImageUrl) " +
           "FROM UserFollow uf JOIN uf.follower u WHERE uf.following.id = :userId " +
           "ORDER BY uf.createdAt DESC")
    List<com.jerrygram.application.dtos.SimpleUserDto> getFollowers(@Param("userId") UUID userId);
    
    /**
     * Get followings (matching .NET GetFollowingsAsync)
     */
    @Query("SELECT new com.jerrygram.application.dtos.SimpleUserDto(u.id, u.username, u.profileImageUrl) " +
           "FROM UserFollow uf JOIN uf.following u WHERE uf.follower.id = :userId " +
           "ORDER BY uf.createdAt DESC")
    List<com.jerrygram.application.dtos.SimpleUserDto> getFollowings(@Param("userId") UUID userId);
}