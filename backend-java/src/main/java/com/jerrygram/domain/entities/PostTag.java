package com.jerrygram.domain.entities;

import jakarta.persistence.*;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.UUID;

@Entity
@Table(name = "\"PostTags\"")
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
@IdClass(PostTagId.class)
public class PostTag {
    
    @Id
    @Column(name = "\"PostId\"", nullable = false)
    private UUID postId;
    
    @Id
    @Column(name = "\"TagId\"", nullable = false)
    private UUID tagId;
    
    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "\"PostId\"", nullable = false, insertable = false, updatable = false)
    private Post post;
    
    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "\"TagId\"", nullable = false, insertable = false, updatable = false)
    private Tag tag;
}