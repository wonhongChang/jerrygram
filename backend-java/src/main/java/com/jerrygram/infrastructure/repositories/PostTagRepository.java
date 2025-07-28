package com.jerrygram.infrastructure.repositories;

import com.jerrygram.domain.entities.PostTag;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

@Repository
public interface PostTagRepository extends JpaRepository<PostTag, UUID> {
    
    /**
     * Find tags by post ID
     */
    List<PostTag> findByPostId(UUID postId);
    
    /**
     * Find PostTag by post ID and tag ID
     */
    Optional<PostTag> findByPostIdAndTagId(UUID postId, UUID tagId);
    
    /**
     * Find posts by tag name
     */
    @Query("SELECT pt.postId FROM PostTag pt JOIN pt.tag t WHERE t.name = :tagName")
    List<UUID> findPostIdsByTagName(@Param("tagName") String tagName);
    
    /**
     * Get popular tags
     */
    @Query("SELECT t.name, COUNT(pt) as count FROM PostTag pt JOIN pt.tag t GROUP BY t.name ORDER BY count DESC")
    List<Object[]> findPopularTags();
    
    /**
     * Search tags by name prefix
     */
    @Query("SELECT DISTINCT t.name FROM PostTag pt JOIN pt.tag t WHERE LOWER(t.name) LIKE LOWER(CONCAT(:prefix, '%')) ORDER BY t.name")
    List<String> findTagNamesByPrefix(@Param("prefix") String prefix);
    
    /**
     * Find hashtags by prefix for autocomplete
     */
    default List<String> findHashtagsByPrefix(String prefix) {
        return findTagNamesByPrefix(prefix);
    }
    
    /**
     * Get post IDs by tag name
     */
    default List<UUID> getPostIdsByTag(String tagName) {
        return findPostIdsByTagName(tagName);
    }
    
    /**
     * Get tag usage count
     */
    @Query("SELECT COUNT(pt) FROM PostTag pt JOIN pt.tag t WHERE t.name = :tagName")
    Long getTagUsageCount(@Param("tagName") String tagName);
    
    /**
     * Delete tags by post ID
     */
    void deleteByPostId(UUID postId);
}