using Persistence.Data;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    public class PostRepository : Repository<Post>, IPostRepository
    {
        public PostRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Post?> GetPostWithUserAndLikesAsync(Guid postId)
        {
            return await _dbSet
                .Include(p => p.User)
                .Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == postId);
        }

        public async Task<Post?> GetPostWithDetailsAsync(Guid postId)
        {
            return await _dbSet
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == postId);
        }

        public async Task<Post?> GetPostWithUserAndTagsAsync(Guid postId)
        {
            return await _dbSet
                .Include(p => p.User)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == postId);
        }

        public async Task<Post?> GetPostWithUserAsync(Guid postId)
        {
            return await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == postId);
        }

        public async Task<PagedResult<PostListItemDto>> GetPublicPostsAsync(Guid? currentUserId, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            
            var baseQuery = _dbSet
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Where(p => p.Visibility == PostVisibility.Public)
                .OrderByDescending(p => p.CreatedAt);

            var totalCount = await baseQuery.CountAsync();

            var posts = await baseQuery
                .Skip(skip)
                .Take(pageSize)
                .Select(p => new PostListItemDto
                {
                    Id = p.Id,
                    Caption = p.Caption,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    Likes = p.Likes.Count,
                    Liked = currentUserId != null && p.Likes.Any(l => l.UserId == currentUserId),
                    User = new SimpleUserDto
                    {
                        Id = p.User.Id,
                        Username = p.User.Username,
                        ProfileImageUrl = p.User.ProfileImageUrl
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

        public async Task<PostListItemDto?> GetPostDtoAsync(Guid postId, Guid? currentUserId)
        {
            return await _dbSet
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Where(p => p.Id == postId)
                .Select(p => new PostListItemDto
                {
                    Id = p.Id,
                    Caption = p.Caption,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    Likes = p.Likes.Count,
                    Liked = currentUserId != null && p.Likes.Any(l => l.UserId == currentUserId),
                    User = new SimpleUserDto
                    {
                        Id = p.User.Id,
                        Username = p.User.Username,
                        ProfileImageUrl = p.User.ProfileImageUrl
                    }
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsFollowerAsync(Guid followerId, Guid followingId)
        {
            return await _context.UserFollows
                .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        }

        public async Task<List<PostListItemDto>> GetPopularPostsAsync()
        {
            return await _dbSet
                .Include(p => p.User)
                .Include(p => p.Likes)
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

            return await _dbSet
                .Include(p => p.User)
                .Include(p => p.Likes)
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

        public async Task<PagedResult<PostListItemDto>> GetUserFeedAsync(List<Guid> followingIds, Guid userId, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            var baseQuery = _dbSet
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Where(p => followingIds.Contains(p.UserId))
                .OrderByDescending(p => p.CreatedAt);

            var totalCount = await baseQuery.CountAsync();
            
            var posts = await baseQuery
                .Skip(skip)
                .Take(pageSize)
                .Select(p => new PostListItemDto
                {
                    Id = p.Id,
                    Caption = p.Caption,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    Likes = p.Likes.Count,
                    Liked = p.Likes.Any(l => l.UserId == userId),
                    User = new SimpleUserDto
                    {
                        Id = p.User.Id,
                        Username = p.User.Username,
                        ProfileImageUrl = p.User.ProfileImageUrl
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

        public async Task<PagedResult<SimpleUserDto>> GetPostLikesAsync(Guid postId, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            var baseQuery = _context.PostLikes
                .Include(pl => pl.User)
                .Where(pl => pl.PostId == postId)
                .OrderByDescending(pl => pl.CreatedAt);

            var totalCount = await baseQuery.CountAsync();
            
            var likes = await baseQuery
                .Skip(skip)
                .Take(pageSize)
                .Select(pl => new SimpleUserDto
                {
                    Id = pl.User.Id,
                    Username = pl.User.Username,
                    ProfileImageUrl = pl.User.ProfileImageUrl
                })
                .ToListAsync();

            return new PagedResult<SimpleUserDto>
            {
                Items = likes,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<List<Post>> GetPostsByIdsAsync(List<Guid> postIds)
        {
            return await _dbSet
                .Where(p => postIds.Contains(p.Id))
                .ToListAsync();
        }
    }
}