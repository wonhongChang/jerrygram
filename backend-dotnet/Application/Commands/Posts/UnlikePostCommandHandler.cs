using Application.Interfaces;

namespace Application.Commands.Posts
{
    public class UnlikePostCommandHandler : ICommandHandler<UnlikePostCommand>
    {
        private readonly IPostRepository _postRepository;
        private readonly IPostLikeRepository _postLikeRepository;
        private readonly INotificationRepository _notificationRepository;

        public UnlikePostCommandHandler(
            IPostRepository postRepository,
            IPostLikeRepository postLikeRepository,
            INotificationRepository notificationRepository)
        {
            _postRepository = postRepository;
            _postLikeRepository = postLikeRepository;
            _notificationRepository = notificationRepository;
        }

        public async Task<bool> HandleAsync(UnlikePostCommand command)
        {
            var like = await _postLikeRepository.GetByPostAndUserAsync(command.PostId, command.UserId);

            if (like == null)
                throw new KeyNotFoundException("Like not found.");

            _postLikeRepository.Remove(like);

            var post = await _postRepository.GetByIdAsync(command.PostId);

            if (post != null && post.UserId != command.UserId)
            {
                var notification = await _notificationRepository.GetLikeNotificationAsync(command.PostId, command.UserId, post.UserId);

                if (notification != null)
                    _notificationRepository.Remove(notification);
            }

            await _postLikeRepository.SaveChangesAsync();
            return true;
        }
    }
}