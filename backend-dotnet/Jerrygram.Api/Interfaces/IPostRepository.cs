using Jerrygram.Api.Dtos;

namespace Jerrygram.Api.Interfaces
{
    public interface IPostRepository
    {
        Task<List<PostListItemDto>> GetPopularPostsAsync();
        Task<List<PostListItemDto>> GetPopularPostsNotFollowedAsync(Guid userId);
    }
}
