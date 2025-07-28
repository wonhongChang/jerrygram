package com.jerrygram.presentation.controllers;

import com.jerrygram.application.commands.auth.LoginCommand;
import com.jerrygram.application.commands.auth.LoginCommandHandler;
import com.jerrygram.application.commands.auth.RegisterUserCommand;
import com.jerrygram.application.commands.auth.RegisterUserCommandHandler;
import com.jerrygram.application.common.AuthResult;
import com.jerrygram.application.dtos.LoginDto;
import com.jerrygram.application.dtos.RegisterDto;
import com.jerrygram.application.dtos.TokenResponse;
import com.jerrygram.application.interfaces.ICommandHandler;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/auth")
@Slf4j
public class AuthController {

    @Autowired
    private RegisterUserCommandHandler registerUserCommandHandler;

    @PostMapping("/register")
    public ResponseEntity<AuthResult> register(@RequestBody RegisterDto registerDto) {
        try {
            log.info("Registration request received for email: {}", registerDto.getEmail());
            
            var command = new RegisterUserCommand(registerDto);
            var result = registerUserCommandHandler.handle(command);
            
            log.info("User registered successfully: {}", result.getUser().getUsername());
            return ResponseEntity.ok(result);
        } catch (Exception e) {
            log.error("Error in registration", e);
            throw e;
        }
    }

    @Autowired
    private LoginCommandHandler loginCommandHandler;

    @PostMapping("/login")
    public ResponseEntity<TokenResponse> login(@RequestBody LoginDto loginDto) {
        try {
            log.info("Login request for email: {}", loginDto.getEmail());
            
            var command = new LoginCommand(loginDto);
            var result = loginCommandHandler.handle(command);
            
            log.info("User logged in successfully: {}", result.getUser().getUsername());
            
            // Return only token to match .NET behavior
            var tokenResponse = TokenResponse.builder()
                    .token(result.getToken())
                    .build();
            
            return ResponseEntity.ok(tokenResponse);
        } catch (Exception e) {
            log.error("Error in login", e);
            throw e;
        }
    }
}