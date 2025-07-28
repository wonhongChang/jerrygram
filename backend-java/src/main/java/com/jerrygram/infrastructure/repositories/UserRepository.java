package com.jerrygram.infrastructure.repositories;

import com.jerrygram.application.dtos.UserProfileDto;
import com.jerrygram.domain.entities.User;
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
public interface UserRepository extends JpaRepository<User, UUID> {
    
    /**
     * Find user by email
     */
    Optional<User> findByEmail(String email);
    
    /**
     * Find user by username
     */
    Optional<User> findByUsername(String username);
    
    /**
     * Check if email exists
     */
    boolean existsByEmail(String email);
    
    /**
     * Check if username exists
     */
    boolean existsByUsername(String username);
    
    /**
     * Search users by username prefix (for autocomplete)
     */
    @Query("SELECT u FROM User u WHERE LOWER(u.username) LIKE LOWER(CONCAT(:prefix, '%')) ORDER BY u.username")
    List<User> findByUsernameStartingWithIgnoreCase(@Param("prefix") String prefix, Pageable pageable);
    
    /**
     * Search users by username containing text
     */
    @Query("SELECT u FROM User u WHERE LOWER(u.username) LIKE LOWER(CONCAT('%', :query, '%')) ORDER BY u.username")
    List<User> findByUsernameContainingIgnoreCase(@Param("query") String query, Pageable pageable);
    
    /**
     * Get users with most followers (for explore page)
     */
    @Query("SELECT u FROM User u LEFT JOIN u.followers f GROUP BY u ORDER BY COUNT(f) DESC")
    Page<User> findTopUsersByFollowers(Pageable pageable);
    
    /**
     * Get users followed by specific user
     */
    @Query("SELECT uf.following FROM UserFollow uf WHERE uf.follower.id = :userId")
    List<User> findFollowingUsers(@Param("userId") UUID userId);
    
    /**
     * Get followers of specific user
     */
    @Query("SELECT uf.follower FROM UserFollow uf WHERE uf.following.id = :userId")
    List<User> findFollowers(@Param("userId") UUID userId);
    
    /**
     * Get user statistics
     */
    @Query("SELECT COUNT(u) FROM User u")
    Long getTotalUsersCount();
    
    /**
     * Get user profile by username (matching .NET structure)
     */
    @Query("SELECT new com.jerrygram.application.dtos.UserProfileDto(" +
           "u.id, u.username, u.email, u.profileImageUrl, u.createdAt, " +
           "SIZE(u.followers), SIZE(u.followings)) " +
           "FROM User u WHERE LOWER(u.username) = LOWER(:username)")
    Optional<UserProfileDto> findUserProfileByUsername(@Param("username") String username);
}