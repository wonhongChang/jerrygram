package com.jerrygram.domain.entities;

import jakarta.persistence.*;
import lombok.*;
import org.hibernate.annotations.CreationTimestamp;

import java.time.LocalDateTime;
import java.util.UUID;

@Entity
@Table(name = "\"UserFollows\"", uniqueConstraints = {
    @UniqueConstraint(columnNames = {"\"FollowerId\"", "\"FollowingId\""})
})
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
@EqualsAndHashCode(exclude = {"follower", "following"})
@ToString(exclude = {"follower", "following"})
public class UserFollow {
    
    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    @Column(name = "\"Id\"")
    private UUID id;
    
    @Column(name = "\"FollowerId\"", nullable = false)
    private UUID followerId;
    
    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "\"FollowerId\"", nullable = false, insertable = false, updatable = false)
    private User follower;
    
    @Column(name = "\"FollowingId\"", nullable = false)
    private UUID followingId;
    
    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "\"FollowingId\"", nullable = false, insertable = false, updatable = false)
    private User following;
    
    @CreationTimestamp
    @Column(name = "\"CreatedAt\"", nullable = false)
    private LocalDateTime createdAt;
}