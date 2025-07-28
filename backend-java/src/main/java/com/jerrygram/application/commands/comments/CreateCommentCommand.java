package com.jerrygram.application.commands.comments;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.UUID;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class CreateCommentCommand {
    private UUID postId;
    private UUID authorId;
    private String content;
}