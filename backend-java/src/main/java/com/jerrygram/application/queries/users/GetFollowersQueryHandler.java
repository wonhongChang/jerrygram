package com.jerrygram.application.queries.users;

import com.jerrygram.application.dtos.SimpleUserDto;
import com.jerrygram.application.interfaces.IQueryHandler;
import com.jerrygram.infrastructure.repositories.UserFollowRepository;
import com.jerrygram.infrastructure.repositories.UserRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
@RequiredArgsConstructor
@Slf4j
public class GetFollowersQueryHandler implements IQueryHandler<GetFollowersQuery, List<SimpleUserDto>> {

    private final UserRepository userRepository;
    private final UserFollowRepository userFollowRepository;

    @Override
    public List<SimpleUserDto> handle(GetFollowersQuery query) {
        var userId = query.getUserId();

        var userExists = userRepository.findById(userId).orElse(null);
        if (userExists == null) {
            return List.of();
        }

        return userFollowRepository.getFollowers(userId);
    }
}