package com.jerrygram.domain.entities;

import jakarta.persistence.*;
import lombok.*;
import org.hibernate.annotations.CreationTimestamp;

import java.time.LocalDateTime;
import java.util.UUID;

@Entity
@Table(name = "\"PostLikes\"", uniqueConstraints = {
    @UniqueConstraint(columnNames = {"\"PostId\"", "\"UserId\""})
})
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
@EqualsAndHashCode(exclude = {"post", "user"})
@ToString(exclude = {"post", "user"})
public class PostLike {
    
    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    @Column(name = "\"Id\"")
    private UUID id;
    
    @Column(name = "\"PostId\"", nullable = false)
    private UUID postId;
    
    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "\"PostId\"", insertable = false, updatable = false)
    private Post post;
    
    @Column(name = "\"UserId\"", nullable = false)
    private UUID userId;
    
    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "\"UserId\"", insertable = false, updatable = false)
    private User user;
    
    @CreationTimestamp
    @Column(name = "\"CreatedAt\"", nullable = false)
    private LocalDateTime createdAt;
}