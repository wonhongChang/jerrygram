using Persistence.Data;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    public class UserFollowRepository : Repository<UserFollow>, IUserFollowRepository
    {
        public UserFollowRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Guid>> GetFollowingIdsAsync(Guid userId)
        {
            return await _dbSet
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FollowingId)
                .ToListAsync();
        }

        public async Task<bool> IsFollowingAsync(Guid followerId, Guid followingId)
        {
            return await _dbSet.AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        }

        public async Task<UserFollow?> GetFollowAsync(Guid followerId, Guid followingId)
        {
            return await _dbSet.FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        }

        public async Task<object> GetFollowersAsync(Guid userId)
        {
            return await _dbSet
                .Where(f => f.FollowingId == userId)
                .Include(f => f.Follower)
                .Select(f => new
                {
                    f.Follower.Id,
                    f.Follower.Username,
                    f.Follower.ProfileImageUrl
                })
                .ToListAsync();
        }

        public async Task<object> GetFollowingsAsync(Guid userId)
        {
            return await _dbSet
                .Where(f => f.FollowerId == userId)
                .Include(f => f.Following)
                .Select(f => new
                {
                    f.Following.Id,
                    f.Following.Username,
                    f.Following.ProfileImageUrl
                })
                .ToListAsync();
        }
    }
}