package com.jerrygram.domain.entities;

import com.fasterxml.jackson.annotation.JsonIgnore;
import jakarta.persistence.*;
import lombok.*;
import org.hibernate.annotations.CreationTimestamp;

import java.time.LocalDateTime;
import java.util.UUID;

@Entity
@Table(name = "\"Comments\"", indexes = {
    @Index(name = "idx_comment_post_id", columnList = "\"PostId\""),
    @Index(name = "idx_comment_user_id", columnList = "\"UserId\"")
})
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
@EqualsAndHashCode(exclude = {"post", "user"})
@ToString(exclude = {"post", "user"})
public class Comment {
    
    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    @Column(name = "\"Id\"")
    private UUID id;
    
    @Column(name = "\"Content\"", nullable = false, length = 1000)
    private String content;
    
    @CreationTimestamp
    @Column(name = "\"CreatedAt\"", nullable = false)
    private LocalDateTime createdAt;
    
    @Column(name = "\"UserId\"", nullable = false)
    private UUID userId;
    
    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "\"UserId\"", insertable = false, updatable = false)
    private User user;
    
    @Column(name = "\"PostId\"", nullable = false)
    private UUID postId;
    
    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "\"PostId\"", insertable = false, updatable = false)
    @JsonIgnore
    private Post post;
}