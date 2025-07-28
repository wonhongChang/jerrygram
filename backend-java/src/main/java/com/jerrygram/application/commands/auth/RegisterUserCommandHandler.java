package com.jerrygram.application.commands.auth;

import com.jerrygram.application.common.AuthResult;
import com.jerrygram.application.common.UserIndex;
import com.jerrygram.application.interfaces.ICacheService;
import com.jerrygram.application.interfaces.ICommandHandler;
import com.jerrygram.application.interfaces.IElasticService;
import com.jerrygram.application.interfaces.IJwtService;
import com.jerrygram.domain.entities.User;
import com.jerrygram.infrastructure.repositories.UserRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

@Service
@RequiredArgsConstructor
@Slf4j
public class RegisterUserCommandHandler implements ICommandHandler<RegisterUserCommand, AuthResult> {

    private final UserRepository userRepository;
    private final PasswordEncoder passwordEncoder;
    private final IJwtService jwtService;
    private final ICacheService cacheService;
    private final IElasticService elasticService;

    @Override
    @Transactional
    public AuthResult handle(RegisterUserCommand command) {
        try {
            log.info("=== STEP 1: Starting registration process ===");
            var dto = command.getRegisterDto();
            
            log.info("=== STEP 2: Got DTO, email: {} ===", dto.getEmail());

            // Create new user using builder to avoid initialization issues
            log.info("=== STEP 3: Creating new user with builder ===");
            var user = User.builder()
                    .email(dto.getEmail())
                    .username(dto.getUsername())
                    .passwordHash(passwordEncoder.encode(dto.getPassword()))
                    .build();

            log.info("=== STEP 4: Saving user to database ===");
            userRepository.save(user);
            log.info("=== STEP 4 SUCCESS: User saved with ID: {} ===", user.getId());

            // Generate JWT token
            log.info("=== STEP 5: Generating JWT token ===");
            var token = jwtService.generateToken(user);
            log.info("=== STEP 5 SUCCESS: Token generated ===");
            
            log.info("=== STEP 6: Building AuthResult ===");
            var userInfo = AuthResult.UserInfo.builder()
                    .id(user.getId())
                    .username(user.getUsername())
                    .email(user.getEmail())
                    .profileImageUrl(user.getProfileImageUrl())
                    .createdAt(user.getCreatedAt())
                    .build();
                    
            var result = AuthResult.builder()
                    .token(token)
                    .user(userInfo)
                    .build();
            
            log.info("=== SUCCESS: User {} registered successfully ===", user.getUsername());
            return result;
        } catch (Exception e) {
            log.error("=== ERROR in registration process ===", e);
            throw e;
        }
    }

    private void invalidateUserAutocompleteCaches(String username) {
        try {
            var normalizedUsername = username.toLowerCase();

            if (normalizedUsername.length() >= 2) {
                var firstTwoChars = normalizedUsername.substring(0, 2);
                cacheService.deleteByPattern("autocomplete:" + firstTwoChars + "*");
            }

            if (normalizedUsername.length() >= 1) {
                var firstChar = normalizedUsername.substring(0, 1);
                cacheService.deleteByPattern("autocomplete:" + firstChar + "*");
            }

            log.debug("Invalidated autocomplete caches for new user: {}", username);
        } catch (Exception ex) {
            log.warn("Failed to invalidate autocomplete caches for user: {}", username, ex);
        }
    }
}