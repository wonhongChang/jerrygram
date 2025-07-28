package com.jerrygram.domain.entities;

import com.fasterxml.jackson.annotation.JsonBackReference;
import com.fasterxml.jackson.annotation.JsonManagedReference;
import com.jerrygram.domain.enums.PostVisibility;
import com.jerrygram.domain.valueobjects.PostCaption;
import jakarta.persistence.*;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.hibernate.annotations.CreationTimestamp;
import org.hibernate.annotations.UpdateTimestamp;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

@Entity
@Table(name = "\"Posts\"", indexes = {
    @Index(name = "idx_post_user_id", columnList = "\"UserId\""),
    @Index(name = "idx_post_created_at", columnList = "\"CreatedAt\""),
    @Index(name = "idx_post_visibility", columnList = "\"Visibility\"")
})
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class Post {
    
    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    @Column(name = "\"Id\"")
    private UUID id;
    
    @Column(name = "\"Caption\"", length = 2200)
    private String caption;
    
    // Transient field for PostCaption value object
    @Transient
    private PostCaption postCaption;
    
    @Column(name = "\"ImageUrl\"", length = 300)
    private String imageUrl;
    
    @Enumerated(EnumType.ORDINAL)
    @Column(name = "\"Visibility\"", nullable = false)
    @Builder.Default
    private PostVisibility visibility = PostVisibility.Public;
    
    
    @CreationTimestamp
    @Column(name = "\"CreatedAt\"", nullable = false)
    private LocalDateTime createdAt;
    
    // Foreign Key
    @Column(name = "\"UserId\"", nullable = false)
    private UUID userId;
    
    // Relationships
    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "\"UserId\"", insertable = false, updatable = false)
    @JsonBackReference
    private User user;
    
    @OneToMany(mappedBy = "post", cascade = CascadeType.ALL, fetch = FetchType.LAZY)
    @JsonManagedReference
    @Builder.Default
    private List<PostLike> postLikes = new ArrayList<>();
    
    @OneToMany(mappedBy = "post", cascade = CascadeType.ALL, fetch = FetchType.LAZY)
    @JsonManagedReference
    @Builder.Default
    private List<Comment> comments = new ArrayList<>();
    
    @OneToMany(mappedBy = "post", cascade = CascadeType.ALL, fetch = FetchType.LAZY)
    @Builder.Default
    private List<PostTag> postTags = new ArrayList<>();
    
    
    public boolean isPublic() {
        return this.visibility == PostVisibility.Public;
    }
    
    public boolean isFollowersOnly() {
        return this.visibility == PostVisibility.FollowersOnly;
    }
    
    // PostCaption value object methods
    public PostCaption getPostCaption() {
        if (postCaption == null) {
            postCaption = PostCaption.create(caption);
        }
        return postCaption;
    }
    
    public void setCaption(String caption) {
        this.caption = caption;
        this.postCaption = null; // Reset cached value object
    }
    
    public List<String> getHashtags() {
        return getPostCaption().getHashtags();
    }
    
    public boolean hasHashtags() {
        return getPostCaption().hasHashtags();
    }
    
    // Calculate counts from relationships (to match .NET behavior)
    public int getLikesCount() {
        return postLikes != null ? postLikes.size() : 0;
    }
    
    public int getCommentsCount() {
        return comments != null ? comments.size() : 0;
    }
}