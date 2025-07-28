package com.jerrygram.application.commands.auth;

import com.jerrygram.application.dtos.RegisterDto;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class RegisterUserCommand {
    private RegisterDto registerDto;
}