using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IPostRepository : IRepository<Post>
    {
        Task<Post?> GetPostWithUserAndLikesAsync(Guid postId);
        Task<Post?> GetPostWithDetailsAsync(Guid postId);
        Task<Post?> GetPostWithUserAndTagsAsync(Guid postId);
        Task<PagedResult<PostListItemDto>> GetPublicPostsAsync(Guid? currentUserId, int page, int pageSize);
        Task<PostListItemDto?> GetPostDtoAsync(Guid postId, Guid? currentUserId);
        Task<List<PostListItemDto>> GetPopularPostsAsync();
        Task<List<PostListItemDto>> GetPopularPostsNotFollowedAsync(Guid userId);
        Task<PagedResult<PostListItemDto>> GetUserFeedAsync(List<Guid> followingIds, Guid userId, int page, int pageSize);
        Task<Post?> GetPostWithUserAsync(Guid postId);
        Task<bool> IsFollowerAsync(Guid followerId, Guid followingId);
        Task<List<Post>> GetPostsByIdsAsync(List<Guid> postIds);
    }
}
