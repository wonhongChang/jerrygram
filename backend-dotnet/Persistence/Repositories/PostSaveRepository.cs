using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Persistence.Repositories
{
    public class PostSaveRepository : Repository<PostSave>, IPostSaveRepository
    {
        public PostSaveRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistsAsync(Guid postId, Guid userId)
        {
            return await _dbSet.AnyAsync(save => save.PostId == postId && save.UserId == userId);
        }

        public async Task<PostSave?> GetByPostAndUserAsync(Guid postId, Guid userId)
        {
            return await _dbSet.FirstOrDefaultAsync(save => save.PostId == postId && save.UserId == userId);
        }

        public Task<PostSave> CreateSaveAsync(Guid postId, Guid userId)
        {
            var save = new PostSave
            {
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            Add(save);
            return Task.FromResult(save);
        }

        public async Task<PagedResult<PostListItemDto>> GetSavedPostsAsync(Guid userId, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            var baseQuery = _dbSet
                .Include(save => save.Post)
                    .ThenInclude(post => post.User)
                .Include(save => save.Post)
                    .ThenInclude(post => post.Likes)
                .Where(save =>
                    save.UserId == userId &&
                    (save.Post.Visibility == PostVisibility.Public || save.Post.UserId == userId))
                .OrderByDescending(save => save.CreatedAt);

            var totalCount = await baseQuery.CountAsync();

            var posts = await baseQuery
                .Skip(skip)
                .Take(pageSize)
                .Select(save => new PostListItemDto
                {
                    Id = save.Post.Id,
                    Caption = save.Post.Caption,
                    ImageUrl = save.Post.ImageUrl,
                    CreatedAt = save.Post.CreatedAt,
                    Likes = save.Post.Likes.Count,
                    Liked = save.Post.Likes.Any(like => like.UserId == userId),
                    Saved = true,
                    User = new SimpleUserDto
                    {
                        Id = save.Post.User.Id,
                        Username = save.Post.User.Username,
                        ProfileImageUrl = save.Post.User.ProfileImageUrl
                    }
                })
                .ToListAsync();

            return new PagedResult<PostListItemDto>
            {
                Items = posts,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
