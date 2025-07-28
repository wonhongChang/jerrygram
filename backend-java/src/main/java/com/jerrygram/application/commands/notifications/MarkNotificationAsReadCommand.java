package com.jerrygram.application.commands.notifications;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.UUID;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class MarkNotificationAsReadCommand {
    private UUID notificationId;
    private UUID userId; // For authorization
}