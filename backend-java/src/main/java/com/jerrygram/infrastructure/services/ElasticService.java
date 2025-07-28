package com.jerrygram.infrastructure.services;

import com.jerrygram.application.common.PostIndex;
import com.jerrygram.application.common.TagIndex;
import com.jerrygram.application.common.UserIndex;
import com.jerrygram.application.interfaces.IElasticService;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.data.elasticsearch.core.ElasticsearchOperations;
import org.springframework.data.elasticsearch.core.SearchHits;
import org.springframework.data.elasticsearch.core.mapping.IndexCoordinates;
import org.springframework.data.elasticsearch.core.query.Criteria;
import org.springframework.data.elasticsearch.core.query.CriteriaQuery;
import org.springframework.data.elasticsearch.core.query.Query;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
@RequiredArgsConstructor
@Slf4j
public class ElasticService implements IElasticService {

    private final ElasticsearchOperations elasticsearchOperations;

    @Override
    public void indexUser(UserIndex userIndex) {
        try {
            elasticsearchOperations.save(userIndex);
            log.debug("Indexed user: {}", userIndex.getId());
        } catch (Exception e) {
            log.error("Failed to index user: {}", userIndex.getId(), e);
        }
    }

    @Override
    public void indexPost(PostIndex postIndex) {
        try {
            elasticsearchOperations.save(postIndex);
            log.debug("Indexed post: {}", postIndex.getId());
        } catch (Exception e) {
            log.error("Failed to index post: {}", postIndex.getId(), e);
        }
    }

    @Override
    public void indexTag(TagIndex tagIndex) {
        try {
            elasticsearchOperations.save(tagIndex);
            log.debug("Indexed tag: {}", tagIndex.getName());
        } catch (Exception e) {
            log.error("Failed to index tag: {}", tagIndex.getName(), e);
        }
    }

    @Override
    public void updateUser(UserIndex userIndex) {
        try {
            elasticsearchOperations.save(userIndex);
            log.debug("Updated user index: {}", userIndex.getId());
        } catch (Exception e) {
            log.error("Failed to update user index: {}", userIndex.getId(), e);
        }
    }

    @Override
    public void updatePost(PostIndex postIndex) {
        try {
            elasticsearchOperations.save(postIndex);
            log.debug("Updated post index: {}", postIndex.getId());
        } catch (Exception e) {
            log.error("Failed to update post index: {}", postIndex.getId(), e);
        }
    }

    @Override
    public void deletePost(String postId) {
        try {
            elasticsearchOperations.delete(postId, IndexCoordinates.of("posts"));
            log.debug("Deleted post from index: {}", postId);
        } catch (Exception e) {
            log.error("Failed to delete post from index: {}", postId, e);
        }
    }

    @Override
    public List<PostIndex> searchPosts(String query) {
        try {
            Criteria criteria = new Criteria("content").contains(query);
            Query searchQuery = new CriteriaQuery(criteria);
            
            SearchHits<PostIndex> searchHits = elasticsearchOperations.search(searchQuery, PostIndex.class);
            return searchHits.stream()
                    .map(hit -> hit.getContent())
                    .toList();
        } catch (Exception e) {
            log.error("Failed to search posts with query: {}", query, e);
            return List.of();
        }
    }

    @Override
    public List<UserIndex> searchUsers(String query, int size) {
        try {
            Criteria criteria = new Criteria("username").contains(query);
            CriteriaQuery criteriaQuery = new CriteriaQuery(criteria);
            criteriaQuery.setMaxResults(size);
            Query searchQuery = criteriaQuery;
            
            SearchHits<UserIndex> searchHits = elasticsearchOperations.search(searchQuery, UserIndex.class);
            return searchHits.stream()
                    .map(hit -> hit.getContent())
                    .toList();
        } catch (Exception e) {
            log.error("Failed to search users with query: {}", query, e);
            return List.of();
        }
    }

    @Override
    public List<TagIndex> searchTags(String query, int size) {
        try {
            Criteria criteria = new Criteria("name").startsWith(query.toLowerCase());
            CriteriaQuery criteriaQuery = new CriteriaQuery(criteria);
            criteriaQuery.setMaxResults(size);
            Query searchQuery = criteriaQuery;
            
            SearchHits<TagIndex> searchHits = elasticsearchOperations.search(searchQuery, TagIndex.class);
            return searchHits.stream()
                    .map(hit -> hit.getContent())
                    .toList();
        } catch (Exception e) {
            log.error("Failed to search tags with query: {}", query, e);
            return List.of();
        }
    }
}