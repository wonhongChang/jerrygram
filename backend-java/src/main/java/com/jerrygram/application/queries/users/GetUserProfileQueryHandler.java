package com.jerrygram.application.queries.users;

import com.jerrygram.application.interfaces.IQueryHandler;
import com.jerrygram.infrastructure.repositories.UserRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;

import java.util.NoSuchElementException;

@Service
@RequiredArgsConstructor
@Slf4j
public class GetUserProfileQueryHandler implements IQueryHandler<GetUserProfileQuery, Object> {

    private final UserRepository userRepository;

    @Override
    public Object handle(GetUserProfileQuery query) {
        if (query.getUsername() != null) {
            return userRepository.findUserProfileByUsername(query.getUsername())
                    .orElseThrow(() -> new NoSuchElementException("User with username '" + query.getUsername() + "' not found"));
        } else if (query.getUserId() != null) {
            var user = userRepository.findById(query.getUserId())
                    .orElseThrow(() -> new NoSuchElementException("User with id '" + query.getUserId() + "' not found"));
            return userRepository.findUserProfileByUsername(user.getUsername())
                    .orElseThrow(() -> new NoSuchElementException("User profile not found"));
        } else {
            throw new IllegalArgumentException("Either username or userId must be provided");
        }
    }
}