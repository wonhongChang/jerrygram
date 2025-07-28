package com.jerrygram.application.dtos;

import com.fasterxml.jackson.annotation.JsonFormat;
import com.jerrygram.domain.enums.PostVisibility;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.time.LocalDateTime;
import java.util.UUID;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class PostDto {
    private UUID id;
    private String caption;
    private String imageUrl;
    private PostVisibility visibility;
    private UserProfileDto author;
    private int likesCount;
    private int commentsCount;
    private boolean isLikedByCurrentUser;
    
    @JsonFormat(pattern = "yyyy-MM-dd'T'HH:mm:ss.SSS'Z'", timezone = "UTC")
    private LocalDateTime createdAt;
    
    @JsonFormat(pattern = "yyyy-MM-dd'T'HH:mm:ss.SSS'Z'", timezone = "UTC")
    private LocalDateTime updatedAt;
}