package com.jerrygram.application.commands.users;

import com.jerrygram.application.common.BlobContainers;
import com.jerrygram.application.common.UserIndex;
import com.jerrygram.application.interfaces.IBlobService;
import com.jerrygram.application.interfaces.ICommandHandler;
import com.jerrygram.application.interfaces.IElasticService;
import com.jerrygram.infrastructure.exceptions.ResourceNotFoundException;
import com.jerrygram.infrastructure.repositories.UserRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.Map;

@Service
@RequiredArgsConstructor
@Slf4j
public class UploadAvatarCommandHandler implements ICommandHandler<UploadAvatarCommand, Map<String, String>> {

    private final UserRepository userRepository;
    private final IBlobService blobService;
    private final IElasticService elasticService;

    @Override
    @Transactional
    public Map<String, String> handle(UploadAvatarCommand command) {
        var userId = command.getUserId();
        var dto = command.getUploadAvatarDto();
        
        log.info("Uploading avatar for user: {}", userId);

        var user = userRepository.findById(userId)
                .orElseThrow(() -> new ResourceNotFoundException("User not found"));

        // Delete old avatar if exists
        if (user.getProfileImageUrl() != null && !user.getProfileImageUrl().isEmpty()) {
            try {
                blobService.delete(user.getProfileImageUrl(), BlobContainers.PROFILES);
                log.info("Deleted old avatar for user: {}", userId);
            } catch (Exception e) {
                log.warn("Failed to delete old avatar for user: {}", userId, e);
            }
        }

        // Upload new avatar
        var imageUrl = blobService.upload(dto.getAvatar(), BlobContainers.PROFILES);
        user.setProfileImageUrl(imageUrl);

        userRepository.save(user);

        // Update Elasticsearch index
        try {
            elasticService.updateUser(UserIndex.builder()
                    .id(user.getId().toString())
                    .username(user.getUsername())
                    .email(user.getEmail())
                    .profileImageUrl(user.getProfileImageUrl())
                    .followersCount(user.getFollowersCount())
                    .followingCount(user.getFollowingCount())
                    .postsCount(user.getPostsCount())
                    .createdAt(user.getCreatedAt())
                    .isVerified(false)
                    .build());
        } catch (Exception e) {
            log.warn("Failed to update user in search index: {}", userId, e);
        }

        log.info("Avatar uploaded successfully for user: {}", userId);
        return Map.of("imageUrl", imageUrl);
    }
}