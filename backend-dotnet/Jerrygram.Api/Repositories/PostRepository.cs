using Jerrygram.Api.Data;
using Jerrygram.Api.Dtos;
using Jerrygram.Api.Interfaces;
using Jerrygram.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Jerrygram.Api.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly AppDbContext _context;

        public PostRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PostListItemDto>> GetPopularPostsAsync()
        {
            return await _context.Posts
                .Where(p => p.Visibility == PostVisibility.Public)
                .OrderByDescending(p => p.Likes.Count)
                .Take(50)
                .Select(p => new PostListItemDto
                {
                    Id = p.Id,
                    Caption = p.Caption,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    Likes = p.Likes.Count,
                    Liked = false,
                    User = new SimpleUserDto
                    {
                        Id = p.User.Id,
                        Username = p.User.Username,
                        ProfileImageUrl = p.User.ProfileImageUrl
                    }
                })
                .ToListAsync();
        }

        public async Task<List<PostListItemDto>> GetPopularPostsNotFollowedAsync(Guid userId)
        {
            var followees = await _context.UserFollows
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FollowingId)
                .ToListAsync();

            return await _context.Posts
                .Where(p => p.Visibility == PostVisibility.Public && !followees.Contains(p.UserId))
                .OrderByDescending(p => p.Likes.Count)
                .Take(50)
                .Select(p => new PostListItemDto
                {
                    Id = p.Id,
                    Caption = p.Caption,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    Likes = p.Likes.Count,
                    Liked = false,
                    User = new SimpleUserDto
                    {
                        Id = p.User.Id,
                        Username = p.User.Username,
                        ProfileImageUrl = p.User.ProfileImageUrl
                    }
                })
                .ToListAsync();
        }
    }
}
