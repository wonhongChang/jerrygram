package com.jerrygram.infrastructure.repositories;

import com.jerrygram.application.dtos.PagedResult;
import com.jerrygram.application.dtos.PostListItemDto;
import com.jerrygram.domain.entities.Post;
import com.jerrygram.domain.enums.PostVisibility;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.UUID;

@Repository
public interface PostRepository extends JpaRepository<Post, UUID> {
    
    /**
     * Find posts by author
     */
    Page<Post> findByUserIdOrderByCreatedAtDesc(UUID userId, Pageable pageable);
    
    /**
     * Find posts by IDs
     */
    List<Post> findByIdIn(List<UUID> postIds);
    
    /**
     * Find public posts for explore/feed
     */
    @Query("SELECT p FROM Post p WHERE p.visibility = :visibility ORDER BY p.createdAt DESC")
    Page<Post> findByVisibility(@Param("visibility") PostVisibility visibility, Pageable pageable);
    
    /**
     * Find posts for user timeline (including followed users)
     */
    @Query("SELECT p FROM Post p WHERE p.user.id IN :userIds AND p.visibility = 0 ORDER BY p.createdAt DESC")
    Page<Post> findTimelinePosts(@Param("userIds") List<UUID> userIds, Pageable pageable);
    
    /**
     * Find posts by hashtag
     */
    @Query("SELECT p FROM Post p WHERE p.caption LIKE %:hashtag% AND p.visibility = 0 ORDER BY p.createdAt DESC")
    Page<Post> findByHashtag(@Param("hashtag") String hashtag, Pageable pageable);
    
    /**
     * Search posts by caption
     */
    @Query("SELECT p FROM Post p WHERE LOWER(p.caption) LIKE LOWER(CONCAT('%', :query, '%')) AND p.visibility = 0 ORDER BY p.createdAt DESC")
    Page<Post> findByCaptionContainingIgnoreCase(@Param("query") String query, Pageable pageable);
    
    /**
     * Get posts with most likes (for explore page)
     */
    @Query("SELECT p FROM Post p LEFT JOIN p.postLikes pl WHERE p.visibility = 0 GROUP BY p ORDER BY COUNT(pl) DESC, p.createdAt DESC")
    Page<Post> findTopPostsByLikes(Pageable pageable);
    
    /**
     * Get post statistics
     */
    @Query("SELECT COUNT(p) FROM Post p WHERE p.visibility = 0")
    Long getTotalPublicPostsCount();
    
    /**
     * Get posts count by user
     */
    @Query("SELECT COUNT(p) FROM Post p WHERE p.user.id = :userId")
    Long getPostsCountByUser(@Param("userId") UUID userId);
    
    /**
     * Get popular posts for explore page (matching .NET GetPopularPostsAsync)
     */
    @Query("SELECT new com.jerrygram.application.dtos.PostListItemDto(" +
           "p.id, p.caption, p.imageUrl, p.createdAt, " +
           "CAST((SELECT COUNT(pl) FROM PostLike pl WHERE pl.post.id = p.id) AS int), " +
           "false, " +
           "new com.jerrygram.application.dtos.SimpleUserDto(p.user.id, p.user.username, p.user.profileImageUrl), " +
           "0.0) " +
           "FROM Post p WHERE p.visibility = 0 " +
           "ORDER BY (SELECT COUNT(pl) FROM PostLike pl WHERE pl.post.id = p.id) DESC, p.createdAt DESC")
    List<PostListItemDto> getPopularPosts();
    
    /**
     * Get popular posts not followed by user (matching .NET GetPopularPostsNotFollowedAsync)
     */
    @Query("SELECT new com.jerrygram.application.dtos.PostListItemDto(" +
           "p.id, p.caption, p.imageUrl, p.createdAt, " +
           "CAST((SELECT COUNT(pl) FROM PostLike pl WHERE pl.post.id = p.id) AS int), " +
           "false, " +
           "new com.jerrygram.application.dtos.SimpleUserDto(p.user.id, p.user.username, p.user.profileImageUrl), " +
           "0.0) " +
           "FROM Post p WHERE p.visibility = 0 " +
           "AND p.user.id NOT IN (" +
           "SELECT uf.following.id FROM UserFollow uf WHERE uf.follower.id = :userId" +
           ") AND p.user.id != :userId " +
           "ORDER BY (SELECT COUNT(pl) FROM PostLike pl WHERE pl.post.id = p.id) DESC, p.createdAt DESC")
    List<PostListItemDto> getPopularPostsNotFollowed(@Param("userId") UUID userId);
    
    /**
     * Get post DTO with like status for authenticated user
     */
    @Query("SELECT new com.jerrygram.application.dtos.PostListItemDto(" +
           "p.id, p.caption, p.imageUrl, p.createdAt, " +
           "CAST((SELECT COUNT(pl) FROM PostLike pl WHERE pl.post.id = p.id) AS int), " +
           "EXISTS(SELECT 1 FROM PostLike pl WHERE pl.post.id = p.id AND pl.user.id = :currentUserId), " +
           "new com.jerrygram.application.dtos.SimpleUserDto(p.user.id, p.user.username, p.user.profileImageUrl), " +
           "0.0) " +
           "FROM Post p WHERE p.id = :postId")
    PostListItemDto getPostDtoForUser(@Param("postId") UUID postId, @Param("currentUserId") UUID currentUserId);
    
    /**
     * Get post DTO for anonymous user
     */
    @Query("SELECT new com.jerrygram.application.dtos.PostListItemDto(" +
           "p.id, p.caption, p.imageUrl, p.createdAt, " +
           "CAST((SELECT COUNT(pl) FROM PostLike pl WHERE pl.post.id = p.id) AS int), " +
           "false, " +
           "new com.jerrygram.application.dtos.SimpleUserDto(p.user.id, p.user.username, p.user.profileImageUrl), " +
           "0.0) " +
           "FROM Post p WHERE p.id = :postId")
    PostListItemDto getPostDtoAnonymous(@Param("postId") UUID postId);
    
    default PostListItemDto getPostDto(UUID postId, UUID currentUserId) {
        if (currentUserId != null) {
            return getPostDtoForUser(postId, currentUserId);
        } else {
            return getPostDtoAnonymous(postId);
        }
    }
    
    /**
     * Get post with user and likes for visibility checking (matching .NET GetPostWithUserAndLikesAsync)
     */
    @Query("SELECT p FROM Post p JOIN FETCH p.user LEFT JOIN FETCH p.postLikes WHERE p.id = :postId")
    Post getPostWithUserAndLikes(@Param("postId") UUID postId);
    
    /**
     * Get public posts with pagination for authenticated users
     */
    @Query("SELECT new com.jerrygram.application.dtos.PostListItemDto(" +
           "p.id, p.caption, p.imageUrl, p.createdAt, " +
           "CAST((SELECT COUNT(pl) FROM PostLike pl WHERE pl.post.id = p.id) AS int), " +
           "EXISTS(SELECT 1 FROM PostLike pl WHERE pl.post.id = p.id AND pl.user.id = :currentUserId), " +
           "new com.jerrygram.application.dtos.SimpleUserDto(p.user.id, p.user.username, p.user.profileImageUrl), " +
           "0.0) " +
           "FROM Post p WHERE p.visibility = 0 " +
           "ORDER BY p.createdAt DESC")
    List<PostListItemDto> getPublicPostsItemsForUser(@Param("currentUserId") UUID currentUserId, Pageable pageable);
    
    /**
     * Get public posts with pagination for anonymous users
     */
    @Query("SELECT new com.jerrygram.application.dtos.PostListItemDto(" +
           "p.id, p.caption, p.imageUrl, p.createdAt, " +
           "CAST((SELECT COUNT(pl) FROM PostLike pl WHERE pl.post.id = p.id) AS int), " +
           "false, " +
           "new com.jerrygram.application.dtos.SimpleUserDto(p.user.id, p.user.username, p.user.profileImageUrl), " +
           "0.0) " +
           "FROM Post p WHERE p.visibility = 0 " +
           "ORDER BY p.createdAt DESC")
    List<PostListItemDto> getPublicPostsItemsAnonymous(Pageable pageable);
    
    /**
     * Count public posts
     */
    @Query("SELECT COUNT(p) FROM Post p WHERE p.visibility = 0")
    long countPublicPosts();
    
    default PagedResult<PostListItemDto> getPublicPosts(UUID currentUserId, int page, int pageSize) {
        var pageable = org.springframework.data.domain.PageRequest.of(page, pageSize);
        List<PostListItemDto> items;
        
        if (currentUserId != null) {
            items = getPublicPostsItemsForUser(currentUserId, pageable);
        } else {
            items = getPublicPostsItemsAnonymous(pageable);
        }
        
        var totalCount = countPublicPosts();
        
        return PagedResult.<PostListItemDto>builder()
                .items(items)
                .totalCount((int) totalCount)
                .page(page)
                .pageSize(pageSize)
                .build();
    }
    
    /**
     * Get user feed posts with pagination (matching .NET GetUserFeedAsync)
     */
    @Query("SELECT new com.jerrygram.application.dtos.PostListItemDto(" +
           "p.id, p.caption, p.imageUrl, p.createdAt, " +
           "CAST((SELECT COUNT(pl) FROM PostLike pl WHERE pl.post.id = p.id) AS int), " +
           "EXISTS(SELECT 1 FROM PostLike pl WHERE pl.post.id = p.id AND pl.user.id = :currentUserId), " +
           "new com.jerrygram.application.dtos.SimpleUserDto(p.user.id, p.user.username, p.user.profileImageUrl), " +
           "0.0) " +
           "FROM Post p WHERE p.user.id IN :followingIds AND p.visibility = 0 " +
           "ORDER BY p.createdAt DESC")
    List<PostListItemDto> getUserFeedItems(@Param("followingIds") List<UUID> followingIds, @Param("currentUserId") UUID currentUserId, Pageable pageable);
    
    /**
     * Count user feed posts
     */
    @Query("SELECT COUNT(p) FROM Post p WHERE p.user.id IN :followingIds AND p.visibility = 0")
    long countUserFeedPosts(@Param("followingIds") List<UUID> followingIds);
    
    default PagedResult<PostListItemDto> getUserFeed(List<UUID> followingIds, UUID currentUserId, int page, int pageSize) {
        var pageable = org.springframework.data.domain.PageRequest.of(page, pageSize);
        var items = getUserFeedItems(followingIds, currentUserId, pageable);
        var totalCount = countUserFeedPosts(followingIds);
        
        return PagedResult.<PostListItemDto>builder()
                .items(items)
                .totalCount((int) totalCount)
                .page(page)
                .pageSize(pageSize)
                .build();
    }
}