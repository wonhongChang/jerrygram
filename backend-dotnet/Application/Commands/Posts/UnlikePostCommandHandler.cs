using Application.Interfaces;

namespace Application.Commands.Posts
{
    public class UnlikePostCommandHandler : ICommandHandler<UnlikePostCommand>
    {
        private readonly IPostRepository _postRepository;
        private readonly IPostLikeRepository _postLikeRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly ICacheService _cacheService;

        public UnlikePostCommandHandler(
            IPostRepository postRepository,
            IPostLikeRepository postLikeRepository,
            INotificationRepository notificationRepository,
            ICacheService cacheService)
        {
            _postRepository = postRepository;
            _postLikeRepository = postLikeRepository;
            _notificationRepository = notificationRepository;
            _cacheService = cacheService;
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

            _cacheService.RemoveByPattern($"post_details_{command.PostId}");
            _cacheService.RemoveByPattern($"post_likes_{command.PostId}");
            _cacheService.RemoveByPattern($"user_feed_{command.UserId}");
            _cacheService.RemoveByPattern("public_posts");
            _cacheService.RemoveByPattern("user_feed");
            _cacheService.RemoveByPattern("explore_posts");
            _cacheService.RemoveByPattern("saved_posts");

            return true;
        }
    }
}
