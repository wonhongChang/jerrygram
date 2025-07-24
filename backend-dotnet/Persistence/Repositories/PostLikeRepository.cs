using Persistence.Data;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    public class PostLikeRepository : Repository<PostLike>, IPostLikeRepository
    {
        public PostLikeRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistsAsync(Guid postId, Guid userId)
        {
            return await _dbSet.AnyAsync(l => l.PostId == postId && l.UserId == userId);
        }

        public async Task<PostLike?> GetLikeAsync(Guid postId, Guid userId)
        {
            return await _dbSet.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
        }

        public async Task<PostLike?> GetByPostAndUserAsync(Guid postId, Guid userId)
        {
            return await _dbSet.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
        }

        public async Task<PostLike> CreateLikeAsync(Guid postId, Guid userId)
        {
            var like = new PostLike
            {
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            
            Add(like);
            return like;
        }

        public async Task<PagedResult<SimpleUserDto>> GetPostLikesAsync(Guid postId, int page, int pageSize)
        {
            var query = _dbSet
                .Where(l => l.PostId == postId)
                .Include(l => l.User)
                .OrderByDescending(l => l.CreatedAt);

            var totalCount = await query.CountAsync();

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new SimpleUserDto
                {
                    Id = l.User.Id,
                    Username = l.User.Username,
                    ProfileImageUrl = l.User.ProfileImageUrl
                })
                .ToListAsync();

            return new PagedResult<SimpleUserDto>
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = users
            };
        }
    }
}