package com.jerrygram.application.commands.comments;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.UUID;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class DeleteCommentCommand {
    private UUID commentId;
    private UUID userId; // For authorization
}