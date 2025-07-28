using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Posts
{
    public class LikePostCommandHandler : ICommandHandler<LikePostCommand>
    {
        private readonly IPostRepository _postRepository;
        private readonly IPostLikeRepository _postLikeRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<LikePostCommandHandler> _logger;

        public LikePostCommandHandler(
            IPostRepository postRepository,
            IPostLikeRepository postLikeRepository,
            ICacheService cacheService,
            ILogger<LikePostCommandHandler> logger)
        {
            _postRepository = postRepository;
            _postLikeRepository = postLikeRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<bool> HandleAsync(LikePostCommand command)
        {
            _logger.LogInformation("User {UserId} attempting to like post {PostId}", 
                command.UserId, command.PostId);

            // Check if post exists
            var post = await _postRepository.GetByIdAsync(command.PostId);
            if (post == null)
            {
                throw new KeyNotFoundException("Post not found");
            }

            // Check if already liked
            var alreadyLiked = await _postLikeRepository.ExistsAsync(command.PostId, command.UserId);
            if (alreadyLiked)
            {
                throw new InvalidOperationException("Post already liked by user");
            }

            // Create like
            await _postLikeRepository.CreateLikeAsync(command.PostId, command.UserId);
            await _postLikeRepository.SaveChangesAsync();

            // Clear relevant caches
            await _cacheService.RemoveAsync($"post_details_{command.PostId}");
            _cacheService.RemoveByPattern($"post_likes_{command.PostId}");
            _cacheService.RemoveByPattern($"user_feed_{command.UserId}");

            _logger.LogInformation("Post {PostId} liked successfully by user {UserId}", 
                command.PostId, command.UserId);

            return true;
        }
    }
}