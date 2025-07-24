using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUserFollowRepository : IRepository<UserFollow>
    {
        Task<List<Guid>> GetFollowingIdsAsync(Guid userId);
        Task<bool> IsFollowingAsync(Guid followerId, Guid followingId);
        Task<UserFollow?> GetFollowAsync(Guid followerId, Guid followingId);
        Task<object> GetFollowersAsync(Guid userId);
        Task<object> GetFollowingsAsync(Guid userId);
    }
}