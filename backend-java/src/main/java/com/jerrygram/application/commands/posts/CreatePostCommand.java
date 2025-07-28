package com.jerrygram.application.commands.posts;

import com.jerrygram.application.dtos.CreatePostDto;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.UUID;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class CreatePostCommand {
    private CreatePostDto createPostDto;
    private UUID authorId;
}