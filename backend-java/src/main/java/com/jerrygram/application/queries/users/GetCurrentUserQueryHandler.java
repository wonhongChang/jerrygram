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
public class GetCurrentUserQueryHandler implements IQueryHandler<GetCurrentUserQuery, UserProfileDto> {

    private final UserRepository userRepository;

    @Override
    public UserProfileDto handle(GetCurrentUserQuery query) {
        var userId = query.getUserId();

        var user = userRepository.findById(userId).orElse(null);

        if (user == null) {
            return null;
        }

        return UserProfileDto.builder()
                .id(user.getId())
                .username(user.getUsername())
                .email(user.getEmail())
                .profileImageUrl(user.getProfileImageUrl())
                .createdAt(user.getCreatedAt())
                .followers(user.getFollowersCount())
                .followings(user.getFollowingCount())
                .build();
    }
}