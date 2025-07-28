package com.jerrygram.infrastructure.repositories;

import com.jerrygram.domain.entities.Tag;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.Optional;
import java.util.UUID;

@Repository
public interface TagRepository extends JpaRepository<Tag, UUID> {
    
    /**
     * Find tag by name
     */
    Optional<Tag> findByName(String name);
    
    /**
     * Check if tag exists by name
     */
    boolean existsByName(String name);
}