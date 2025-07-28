package com.jerrygram.domain.entities;

import com.fasterxml.jackson.annotation.JsonManagedReference;
import jakarta.persistence.*;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;
import lombok.ToString;
import org.hibernate.annotations.CreationTimestamp;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

@Entity
@Table(name = "\"Users\"", indexes = {
    @Index(name = "idx_user_email", columnList = "\"Email\""),
    @Index(name = "idx_user_username", columnList = "\"Username\"")
})
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class User {
    
    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    @Column(name = "\"Id\"")
    private UUID id;
    
    @Column(name = "\"Email\"", nullable = false, unique = true, length = 100)
    private String email;
    
    @Column(name = "\"Username\"", nullable = false, unique = true, length = 30)
    private String username;
    
    @Column(name = "\"PasswordHash\"", nullable = false)
    private String passwordHash;
    
    @Column(name = "\"ProfileImageUrl\"", length = 300)
    private String profileImageUrl;
    
    
    @CreationTimestamp
    @Column(name = "\"CreatedAt\"", nullable = false)
    private LocalDateTime createdAt;
    
    
    // Relationships
    @OneToMany(mappedBy = "user", cascade = CascadeType.ALL, fetch = FetchType.LAZY)
    @JsonManagedReference
    @Builder.Default
    @ToString.Exclude
    private List<Post> posts = new ArrayList<>();
    
    @OneToMany(mappedBy = "user", cascade = CascadeType.ALL, fetch = FetchType.LAZY)
    @Builder.Default
    @ToString.Exclude
    private List<PostLike> likes = new ArrayList<>();
    
    @OneToMany(mappedBy = "user", cascade = CascadeType.ALL, fetch = FetchType.LAZY)
    @Builder.Default
    @ToString.Exclude
    private List<Comment> comments = new ArrayList<>();
    
    @OneToMany(mappedBy = "follower", cascade = CascadeType.ALL, fetch = FetchType.LAZY)
    @Builder.Default
    @ToString.Exclude
    private List<UserFollow> followings = new ArrayList<>();
    
    @OneToMany(mappedBy = "following", cascade = CascadeType.ALL, fetch = FetchType.LAZY)
    @Builder.Default
    @ToString.Exclude
    private List<UserFollow> followers = new ArrayList<>();
    
    @OneToMany(mappedBy = "recipient", cascade = CascadeType.ALL, fetch = FetchType.LAZY)
    @Builder.Default
    @ToString.Exclude
    private List<Notification> notifications = new ArrayList<>();
    
    // Calculate counts from relationships (to match .NET behavior)
    public int getFollowersCount() {
        return followers != null ? followers.size() : 0;
    }
    
    public int getFollowingCount() {
        return followings != null ? followings.size() : 0;
    }
    
    public int getPostsCount() {
        return posts != null ? posts.size() : 0;
    }
}