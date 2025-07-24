using Application.Interfaces;
using Domain.Entities;

namespace Application.Commands.Users
{
    public class UnfollowUserCommandHandler : ICommandHandler<UnfollowUserCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserFollowRepository _userFollowRepository;
        private readonly INotificationRepository _notificationRepository;

        public UnfollowUserCommandHandler(
            IUserRepository userRepository,
            IUserFollowRepository userFollowRepository,
            INotificationRepository notificationRepository)
        {
            _userRepository = userRepository;
            _userFollowRepository = userFollowRepository;
            _notificationRepository = notificationRepository;
        }

        public async Task<bool> HandleAsync(UnfollowUserCommand command)
        {
            if (command.CurrentUserId == command.TargetUserId)
                throw new ArgumentException("Cannot unfollow yourself.");

            var targetUser = await _userRepository.GetByIdAsync(command.TargetUserId);
            if (targetUser == null)
                throw new KeyNotFoundException("User not found.");

            var follow = await _userFollowRepository.GetFollowAsync(command.CurrentUserId, command.TargetUserId);
            if (follow == null)
                throw new InvalidOperationException("Not following this user.");

            _userFollowRepository.Remove(follow);

            // Remove notification
            var notification = await _notificationRepository.GetFollowNotificationAsync(command.CurrentUserId, command.TargetUserId);
            if (notification != null)
                _notificationRepository.Remove(notification);

            await _userFollowRepository.SaveChangesAsync();
            return true;
        }
    }
}