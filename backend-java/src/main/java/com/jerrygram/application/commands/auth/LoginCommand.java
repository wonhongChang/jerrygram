package com.jerrygram.application.commands.auth;

import com.jerrygram.application.dtos.LoginDto;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class LoginCommand {
    private LoginDto loginDto;
}