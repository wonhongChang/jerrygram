using Persistence.Data;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    public class CommentRepository : Repository<Comment>, ICommentRepository
    {
        public CommentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(Guid postId)
        {
            return await _dbSet.Where(c => c.PostId == postId).ToListAsync();
        }

        public async Task<object> GetCommentsWithUsersByPostIdAsync(Guid postId)
        {
            return await _dbSet
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.CreatedAt,
                    User = new
                    {
                        c.User.Id,
                        c.User.Username,
                        c.User.ProfileImageUrl
                    }
                })
                .ToListAsync();
        }
    }
}