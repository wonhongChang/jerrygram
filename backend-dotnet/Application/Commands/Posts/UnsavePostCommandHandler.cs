using Application.Interfaces;

namespace Application.Commands.Posts
{
    public class UnsavePostCommandHandler : ICommandHandler<UnsavePostCommand>
    {
        private readonly IPostSaveRepository _postSaveRepository;
        private readonly ICacheService _cacheService;

        public UnsavePostCommandHandler(
            IPostSaveRepository postSaveRepository,
            ICacheService cacheService)
        {
            _postSaveRepository = postSaveRepository;
            _cacheService = cacheService;
        }

        public async Task<bool> HandleAsync(UnsavePostCommand command)
        {
            var save = await _postSaveRepository.GetByPostAndUserAsync(command.PostId, command.UserId);
            if (save == null)
                return true;

            _postSaveRepository.Remove(save);
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
