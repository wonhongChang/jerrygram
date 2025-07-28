package com.jerrygram.application.commands.users;

import com.jerrygram.application.interfaces.ICommandHandler;
import com.jerrygram.domain.entities.UserFollow;
import com.jerrygram.infrastructure.repositories.UserFollowRepository;
import com.jerrygram.infrastructure.repositories.UserRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

@Service
@RequiredArgsConstructor
@Slf4j
public class FollowUserCommandHandler implements ICommandHandler<FollowUserCommand, Boolean> {

    private final UserFollowRepository userFollowRepository;
    private final UserRepository userRepository;

    @Override
    @Transactional
    public Boolean handle(FollowUserCommand command) {
        var followerId = command.getFollowerId();
        var followedId = command.getFollowedId();
        
        if (followerId.equals(followedId)) {
            throw new IllegalArgumentException("Cannot follow yourself");
        }
        
        log.info("User {} following user {}", followerId, followedId);

        var existingFollow = userFollowRepository.findByFollowerIdAndFollowingId(followerId, followedId);
        
        if (existingFollow.isPresent()) {
            // Unfollow
            userFollowRepository.delete(existingFollow.get());
            
            log.info("User {} unfollowed user {}", followerId, followedId);
            return false;
        } else {
            // Follow
            var follower = userRepository.findById(followerId)
                    .orElseThrow(() -> new IllegalArgumentException("Follower not found"));
            var followed = userRepository.findById(followedId)
                    .orElseThrow(() -> new IllegalArgumentException("Followed user not found"));
            
            var userFollow = UserFollow.builder()
                    .followerId(followerId)
                    .followingId(followedId)
                    .follower(follower)
                    .following(followed)
                    .build();
            
            userFollowRepository.save(userFollow);
            
            log.info("User {} followed user {}", followerId, followedId);
            return true;
        }
    }
}