package com.jerrygram.application.commands.users;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.UUID;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class FollowUserCommand {
    private UUID followerId;
    private UUID followedId;
}