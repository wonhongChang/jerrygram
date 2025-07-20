using Jerrygram.Api.Dtos;
using Jerrygram.Api.Models;

namespace Jerrygram.Api.Interfaces
{
    public interface IPostService
    {
        Task<Post> CreatePostAsync(PostUploadDto dto, Guid userId);
        Task<Post> UpdatePostAsync(Guid postId, UpdatePostDto dto, Guid userId);
        Task<PagedResult<PostListItemDto>> GetAllPublicPostsAsync(Guid? currentUserId, int page, int pageSize);
        Task<PostListItemDto> GetPostByIdAsync(Guid postId, Guid? currentUserId);
        Task DeletePostAsync(Guid postId, Guid userId);
        Task LikePostAsync(Guid postId, Guid userId);
        Task UnlikePostAsync(Guid postId, Guid userId);
        Task<PagedResult<PostListItemDto>> GetUserFeedAsync(Guid userId, int page, int pageSize);
        Task<PagedResult<SimpleUserDto>> GetPostLikesAsync(Guid postId, int page, int pageSize);
    }
}
