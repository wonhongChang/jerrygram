using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IPostLikeRepository : IRepository<PostLike>
    {
        Task<bool> ExistsAsync(Guid postId, Guid userId);
        Task<PostLike?> GetLikeAsync(Guid postId, Guid userId);
        Task<PostLike?> GetByPostAndUserAsync(Guid postId, Guid userId);
        Task<PostLike> CreateLikeAsync(Guid postId, Guid userId);
        Task<PagedResult<SimpleUserDto>> GetPostLikesAsync(Guid postId, int page, int pageSize);
    }
}