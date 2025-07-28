package com.jerrygram.application.commands.posts;

import com.jerrygram.application.dtos.UpdatePostDto;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.UUID;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class UpdatePostCommand {
    private UUID postId;
    private UUID userId;
    private UpdatePostDto updatePostDto;
}