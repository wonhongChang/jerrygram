package com.jerrygram.application.commands.posts;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.UUID;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class LikePostCommand {
    private UUID postId;
    private UUID userId;
}