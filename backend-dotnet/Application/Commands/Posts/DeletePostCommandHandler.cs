using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Posts
{
    public class DeletePostCommandHandler : ICommandHandler<DeletePostCommand>
    {
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IPostLikeRepository _postLikeRepository;
        private readonly IPostTagRepository _postTagRepository;
        private readonly IBlobService _blobService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<DeletePostCommandHandler> _logger;

        public DeletePostCommandHandler(
            IPostRepository postRepository,
            ICommentRepository commentRepository,
            IPostLikeRepository postLikeRepository,
            IPostTagRepository postTagRepository,
            IBlobService blobService,
            ICacheService cacheService,
            ILogger<DeletePostCommandHandler> logger)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _postLikeRepository = postLikeRepository;
            _postTagRepository = postTagRepository;
            _blobService = blobService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<bool> HandleAsync(DeletePostCommand command)
        {
            _logger.LogInformation("Attempting to delete post {PostId} by user {UserId}", 
                command.PostId, command.UserId);

            var post = await _postRepository.GetPostWithDetailsAsync(command.PostId);

            if (post == null)
            {
                throw new KeyNotFoundException("Post not found");
            }

            if (post.UserId != command.UserId)
            {
                throw new UnauthorizedAccessException("You can only delete your own posts");
            }

            // Delete image from blob storage if exists
            if (!string.IsNullOrEmpty(post.ImageUrl))
            {
                try
                {
                    await _blobService.DeleteAsync(post.ImageUrl, "posts");
                    _logger.LogInformation("Image deleted from blob storage for post {PostId}", command.PostId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete image from blob storage for post {PostId}", command.PostId);
                    // Continue with post deletion even if blob deletion fails
                }
            }

            // Remove related entities (EF will handle cascade delete, but let's be explicit)
            _postLikeRepository.RemoveRange(post.Likes);
            _commentRepository.RemoveRange(post.Comments);
            _postTagRepository.RemoveRange(post.PostTags);
            _postRepository.Remove(post);

            await _postRepository.SaveChangesAsync();

            // Clear caches
            await _cacheService.RemoveAsync($"post_details_{command.PostId}");
            _cacheService.RemoveByPattern("public_posts");
            _cacheService.RemoveByPattern($"user_feed_{command.UserId}");

            _logger.LogInformation("Post {PostId} deleted successfully", command.PostId);

            return true;
        }
    }
}