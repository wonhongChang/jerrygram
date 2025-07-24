using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Commands.Users
{
    public class FollowUserCommandHandler : ICommandHandler<FollowUserCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserFollowRepository _userFollowRepository;
        private readonly INotificationRepository _notificationRepository;

        public FollowUserCommandHandler(
            IUserRepository userRepository,
            IUserFollowRepository userFollowRepository,
            INotificationRepository notificationRepository)
        {
            _userRepository = userRepository;
            _userFollowRepository = userFollowRepository;
            _notificationRepository = notificationRepository;
        }

        public async Task<bool> HandleAsync(FollowUserCommand command)
        {
            if (command.CurrentUserId == command.TargetUserId)
                throw new ArgumentException("Cannot follow yourself.");

            var targetUser = await _userRepository.GetByIdAsync(command.TargetUserId);
            if (targetUser == null)
                throw new KeyNotFoundException("User not found.");

            var alreadyFollowing = await _userFollowRepository.IsFollowingAsync(command.CurrentUserId, command.TargetUserId);
            if (alreadyFollowing)
                throw new InvalidOperationException("Already following this user.");

            var follow = new UserFollow
            {
                FollowerId = command.CurrentUserId,
                FollowingId = command.TargetUserId,
                CreatedAt = DateTime.UtcNow
            };

            _userFollowRepository.Add(follow);

            // Create notification
            var currentUser = await _userRepository.GetByIdAsync(command.CurrentUserId);
            if (currentUser != null)
            {
                var notification = new Notification
                {
                    RecipientId = command.TargetUserId,
                    FromUserId = command.CurrentUserId,
                    Type = NotificationType.Follow,
                    Message = $"{currentUser.Username} started following you.",
                    CreatedAt = DateTime.UtcNow
                };

                _notificationRepository.Add(notification);
            }

            await _userFollowRepository.SaveChangesAsync();
            return true;
        }
    }
}