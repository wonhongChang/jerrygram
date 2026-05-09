using Application.Interfaces;

namespace Application.Commands.Posts
{
    public class SavePostCommandHandler : ICommandHandler<SavePostCommand>
    {
        private readonly IPostRepository _postRepository;
        private readonly IPostSaveRepository _postSaveRepository;
        private readonly ICacheService _cacheService;

        public SavePostCommandHandler(
            IPostRepository postRepository,
            IPostSaveRepository postSaveRepository,
            ICacheService cacheService)
        {
            _postRepository = postRepository;
            _postSaveRepository = postSaveRepository;
            _cacheService = cacheService;
        }

        public async Task<bool> HandleAsync(SavePostCommand command)
        {
            var post = await _postRepository.GetByIdAsync(command.PostId);
            if (post == null)
                throw new KeyNotFoundException("Post not found.");

            if (await _postSaveRepository.ExistsAsync(command.PostId, command.UserId))
                return true;

            await _postSaveRepository.CreateSaveAsync(command.PostId, command.UserId);
            await _postSaveRepository.SaveChangesAsync();

            InvalidatePostCaches(command.PostId, command.UserId);
            return true;
        }

        private void InvalidatePostCaches(Guid postId, Guid userId)
        {
            _cacheService.RemoveByPattern($"post_details_{postId}");
            _cacheService.RemoveByPattern($"saved_posts_{userId}");
            _cacheService.RemoveByPattern("public_posts");
            _cacheService.RemoveByPattern("user_feed");
            _cacheService.RemoveByPattern("explore_posts");
        }
    }
}
