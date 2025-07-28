package com.jerrygram.domain.entities;

import com.jerrygram.domain.enums.NotificationType;
import jakarta.persistence.*;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.time.LocalDateTime;
import java.util.UUID;

@Entity
@Table(name = "\"Notifications\"", indexes = {
    @Index(name = "idx_notification_recipient", columnList = "\"RecipientId\""),
    @Index(name = "idx_notification_read", columnList = "\"IsRead\""),
    @Index(name = "idx_notification_created", columnList = "\"CreatedAt\"")
})
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class Notification {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    @Column(name = "\"Id\"")
    private UUID id;

    @Column(name = "\"RecipientId\"", nullable = false)
    private UUID recipientId;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "\"RecipientId\"", nullable = false, insertable = false, updatable = false)
    private User recipient;

    @Column(name = "\"FromUserId\"", nullable = false)
    private UUID fromUserId;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "\"FromUserId\"", nullable = false, insertable = false, updatable = false)
    private User fromUser;

    @Enumerated(EnumType.ORDINAL)
    @Column(name = "\"Type\"", nullable = false)
    private NotificationType type;

    @Column(name = "\"PostId\"")
    private UUID postId;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "\"PostId\"", insertable = false, updatable = false)
    private Post post;

    @Column(name = "\"Message\"", length = 500)
    private String message;

    @Column(name = "\"IsRead\"")
    @Builder.Default
    private Boolean isRead = false;

    @Column(name = "\"CreatedAt\"", nullable = false)
    @Builder.Default
    private LocalDateTime createdAt = LocalDateTime.now();

    // Helper methods
    public void markAsRead() {
        this.isRead = true;
    }

    public void markAsUnread() {
        this.isRead = false;
    }
}