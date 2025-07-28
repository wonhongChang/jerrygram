package com.jerrygram.application.commands.auth;

import com.jerrygram.application.common.AuthResult;
import com.jerrygram.application.interfaces.ICommandHandler;
import com.jerrygram.application.interfaces.IJwtService;
import com.jerrygram.domain.entities.User;
import com.jerrygram.infrastructure.repositories.UserRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;

@Service
@RequiredArgsConstructor
@Slf4j
public class LoginCommandHandler implements ICommandHandler<LoginCommand, AuthResult> {

    private final UserRepository userRepository;
    private final PasswordEncoder passwordEncoder;
    private final IJwtService jwtService;

    @Override
    public AuthResult handle(LoginCommand command) {
        var dto = command.getLoginDto();
        
        log.info("Login attempt for email: {}", dto.getEmail());

        User user = userRepository.findByEmail(dto.getEmail())
                .orElseThrow(() -> {
                    log.warn("Login failed: User not found for email: {}", dto.getEmail());
                    return new IllegalArgumentException("Invalid email or password.");
                });

        if (!passwordEncoder.matches(dto.getPassword(), user.getPasswordHash())) {
            log.warn("Login failed: Invalid password for email: {}", dto.getEmail());
            throw new IllegalArgumentException("Invalid email or password.");
        }

        String token = jwtService.generateToken(user);
        
        log.info("User {} logged in successfully with username: {}", user.getId(), user.getUsername());

        var userInfo = AuthResult.UserInfo.builder()
                .id(user.getId())
                .username(user.getUsername())
                .email(user.getEmail())
                .profileImageUrl(user.getProfileImageUrl())
                .createdAt(user.getCreatedAt())
                .build();
                
        return AuthResult.builder()
                .token(token)
                .user(userInfo)
                .build();
    }
}