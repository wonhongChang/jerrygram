package com.jerrygram.presentation.controllers;

import com.jerrygram.application.commands.users.FollowUserCommand;
import com.jerrygram.application.commands.users.UploadAvatarCommand;
import com.jerrygram.application.dtos.UploadAvatarDto;
import com.jerrygram.application.dtos.SimpleUserDto;
import com.jerrygram.application.dtos.UserProfileDto;
import com.jerrygram.application.interfaces.ICommandHandler;
import com.jerrygram.application.interfaces.IQueryHandler;
import com.jerrygram.application.queries.users.GetCurrentUserQuery;
import com.jerrygram.application.queries.users.GetFollowersQuery;
import com.jerrygram.application.queries.users.GetFollowingQuery;
import com.jerrygram.application.queries.users.GetUserProfileByUsernameQuery;
import com.jerrygram.application.queries.users.GetUserProfileQuery;
import com.jerrygram.domain.entities.User;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import java.util.List;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.multipart.MultipartFile;

import java.util.Map;
import java.util.UUID;

@RestController
@RequestMapping("/api/users")
@RequiredArgsConstructor
@Slf4j
public class UsersController {

    private final IQueryHandler<GetCurrentUserQuery, UserProfileDto> getCurrentUserQueryHandler;
    private final IQueryHandler<GetUserProfileQuery, Object> getUserProfileQueryHandler;
    private final IQueryHandler<GetUserProfileByUsernameQuery, UserProfileDto> getUserProfileByUsernameQueryHandler;
    private final IQueryHandler<GetFollowersQuery, List<SimpleUserDto>> getFollowersQueryHandler;
    private final IQueryHandler<GetFollowingQuery, List<SimpleUserDto>> getFollowingQueryHandler;
    private final ICommandHandler<FollowUserCommand, Boolean> followUserCommandHandler;
    private final ICommandHandler<UploadAvatarCommand, Map<String, String>> uploadAvatarCommandHandler;

    @GetMapping("/me")
    public ResponseEntity<UserProfileDto> getCurrentUser(Authentication authentication) {
        User currentUser = (User) authentication.getPrincipal();
        log.info("Getting current user: {}", currentUser.getUsername());
        
        var query = new GetCurrentUserQuery(currentUser.getId());
        var result = getCurrentUserQueryHandler.handle(query);
        
        return ResponseEntity.ok(result);
    }

    @GetMapping("/{userId}")
    public ResponseEntity<Object> getUserProfile(
            @PathVariable UUID userId,
            Authentication authentication) {
        
        User currentUser = (User) authentication.getPrincipal();
        log.info("Getting user profile: {} for current user: {}", userId, currentUser.getUsername());
        
        var query = new GetUserProfileQuery(userId, currentUser.getId());
        var result = getUserProfileQueryHandler.handle(query);
        
        return ResponseEntity.ok(result);
    }

    @GetMapping("/profile/{username}")
    public ResponseEntity<UserProfileDto> getUserProfileByUsername(
            @PathVariable String username,
            Authentication authentication) {
        
        User currentUser = authentication != null ? (User) authentication.getPrincipal() : null;
        var currentUserId = currentUser != null ? currentUser.getId() : null;
        
        log.info("Getting user profile by username: {} for current user: {}", 
                username, currentUserId);
        
        var query = new GetUserProfileByUsernameQuery(username, currentUserId);
        var result = getUserProfileByUsernameQueryHandler.handle(query);
        
        return ResponseEntity.ok(result);
    }

    @PostMapping("/{userId}/follow")
    public ResponseEntity<Boolean> followUser(
            @PathVariable UUID userId,
            Authentication authentication) {
        
        User currentUser = (User) authentication.getPrincipal();
        log.info("User {} toggling follow for user: {}", currentUser.getUsername(), userId);
        
        var command = new FollowUserCommand(currentUser.getId(), userId);
        var isFollowing = followUserCommandHandler.handle(command);
        
        return ResponseEntity.ok(isFollowing);
    }

    @PostMapping("/me/avatar")
    public ResponseEntity<Map<String, String>> uploadAvatar(
            @RequestParam("avatar") MultipartFile avatar,
            Authentication authentication) {
        
        User currentUser = (User) authentication.getPrincipal();
        log.info("User {} uploading avatar", currentUser.getUsername());
        
        var uploadDto = new UploadAvatarDto(avatar);
        var command = new UploadAvatarCommand(currentUser.getId(), uploadDto);
        var result = uploadAvatarCommandHandler.handle(command);
        
        log.info("Avatar uploaded successfully for user: {}", currentUser.getUsername());
        return ResponseEntity.ok(result);
    }

    @GetMapping("/{userId}/followers")
    public ResponseEntity<List<SimpleUserDto>> getFollowers(
            @PathVariable UUID userId,
            @RequestParam(defaultValue = "0") int page,
            @RequestParam(defaultValue = "10") int size,
            Authentication authentication) {
        
        User currentUser = authentication != null ? (User) authentication.getPrincipal() : null;
        log.info("Getting followers for user: {}", userId);
        
        var query = new GetFollowersQuery(userId, page, size);
        var result = getFollowersQueryHandler.handle(query);
        
        log.info("Retrieved {} followers for user: {}", result.size(), userId);
        return ResponseEntity.ok(result);
    }

    @GetMapping("/{userId}/following")
    public ResponseEntity<List<SimpleUserDto>> getFollowing(
            @PathVariable UUID userId,
            @RequestParam(defaultValue = "0") int page,
            @RequestParam(defaultValue = "10") int size,
            Authentication authentication) {
        
        User currentUser = authentication != null ? (User) authentication.getPrincipal() : null;
        log.info("Getting following list for user: {}", userId);
        
        var query = new GetFollowingQuery(userId, page, size);
        var result = getFollowingQueryHandler.handle(query);
        
        log.info("Retrieved {} following for user: {}", result.size(), userId);
        return ResponseEntity.ok(result);
    }
}