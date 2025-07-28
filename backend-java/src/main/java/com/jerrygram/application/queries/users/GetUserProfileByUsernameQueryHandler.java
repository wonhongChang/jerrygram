package com.jerrygram.application.queries.users;

import com.jerrygram.application.dtos.UserProfileDto;
import com.jerrygram.application.interfaces.IQueryHandler;
import com.jerrygram.infrastructure.repositories.UserRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;

@Service
@RequiredArgsConstructor
@Slf4j
public class GetUserProfileByUsernameQueryHandler implements IQueryHandler<GetUserProfileByUsernameQuery, UserProfileDto> {

    private final UserRepository userRepository;

    @Override
    public UserProfileDto handle(GetUserProfileByUsernameQuery query) {
        var username = query.getUsername();
        
        log.info("Getting user profile by username: {}", username);

        return userRepository.findUserProfileByUsername(username)
                .orElseThrow(() -> new IllegalArgumentException("User with username '" + username + "' not found"));
    }
}