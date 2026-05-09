using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IPostSaveRepository : IRepository<PostSave>
    {
        Task<bool> ExistsAsync(Guid postId, Guid userId);
        Task<PostSave?> GetByPostAndUserAsync(Guid postId, Guid userId);
        Task<PostSave> CreateSaveAsync(Guid postId, Guid userId);
        Task<PagedResult<PostListItemDto>> GetSavedPostsAsync(Guid userId, int page, int pageSize);
    }
}
