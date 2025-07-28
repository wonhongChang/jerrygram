package com.jerrygram.application.commands.users;

import com.jerrygram.application.dtos.UploadAvatarDto;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.UUID;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class UploadAvatarCommand {
    private UUID userId;
    private UploadAvatarDto uploadAvatarDto;
}