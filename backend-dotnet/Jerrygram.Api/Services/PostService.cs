using Jerrygram.Api.Constants;
using Jerrygram.Api.Data;
using Jerrygram.Api.Dtos;
using Jerrygram.Api.Interfaces;
using Jerrygram.Api.Models;
using Jerrygram.Api.Search;
using Jerrygram.Api.Search.IndexModels;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Jerrygram.Api.Services
{
    public class PostService : IPostService
    {
        private readonly AppDbContext _context;
        private readonly BlobService _blobService;
        private readonly ElasticService _elastic;

        private readonly IPostRepository _postRepository;
        private readonly RecommendClient _recommendClient;

        public PostService(AppDbContext context, BlobService blobService, ElasticService elastic, IPostRepository postRepository, RecommendClient recommendClient)
        {
            _context = context;
            _blobService = blobService;
            _elastic = elastic;

            _postRepository = postRepository;
            _recommendClient = recommendClient;
        }

        /// <summary>
        /// Creates a new post with the specified caption, image, and visibility settings, and associates it with the
        /// given user.
        /// </summary>
        public async Task<PostListItemDto> CreatePostAsync(PostUploadDto dto, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Caption) && (dto.Image == null || dto.Image.Length == 0))
                throw new ArgumentException("At least one of caption or image must be provided.");

            string? imageUrl = null;
            if (dto.Image != null && dto.Image.Length > 0)
            {
                imageUrl = await _blobService.UploadAsync(dto.Image, BlobContainers.Post);
            }

            var post = new Post
            {
                Id = Guid.NewGuid(),
                Caption = dto.Caption,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                Visibility = dto.Visibility
            };

            _context.Posts.Add(post);

            var hashtags = ExtractHashtags(dto.Caption ?? string.Empty);

            var existingTags = await _context.Tags
                .Where(t => hashtags.Contains(t.Name))
                .ToDictionaryAsync(t => t.Name);

            foreach (var tagName in hashtags)
            {
                if (!existingTags.TryGetValue(tagName, out var tag))
                {
                    tag = new Tag { Id = Guid.NewGuid(), Name = tagName};
                    _context.Tags.Add(tag);
                    existingTags[tagName] = tag;
                }

                _context.PostTags.Add(new PostTag
                    {
                        PostId = post.Id,
                        TagId = tag.Id
                    });

                await _elastic.IndexTagAsync(new TagIndex
                {
                    Id = tag.Id,
                    Name = tag.Name
                });
            }

            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(post.UserId);
            if (user != null)
            {
                await _elastic.IndexPostAsync(new PostIndex
                {
                    Id = post.Id,
                    Caption = post.Caption,
                    UserId = user.Id,
                    Username = user.Username,
                    CreatedAt = post.CreatedAt,
                    Visibility = post.Visibility.ToString()
                });
            }

            return new PostListItemDto
            {
                Id = post.Id,
                Caption = post.Caption,
                ImageUrl = post.ImageUrl,
                CreatedAt = post.CreatedAt,
                Likes = 0,
                Liked = false,
                User = new SimpleUserDto
                {
                    Id = user!.Id,
                    Username = user.Username,
                    ProfileImageUrl = user.ProfileImageUrl
                }
            };
        }

        /// <summary>
        /// Extracts unique hashtags from the specified caption.
        /// </summary>
        /// <remarks>A hashtag is defined as a word starting with the '#' character, followed by letters,
        /// numbers, or underscores. The method performs a case-insensitive comparison and ensures that the returned
        /// list contains distinct hashtags.</remarks>
        private static List<string> ExtractHashtags(string caption)
        {
            return [.. Regex.Matches(caption, @"#([\p{L}\p{N}_]+)")
                .Select(m => m.Groups[1].Value.ToLowerInvariant())
                .Distinct()];
        }

        public async Task<Post> UpdatePostAsync(Guid postId, UpdatePostDto dto, Guid userId)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                throw new KeyNotFoundException("Post not found.");

            if (post.UserId != userId)
                throw new UnauthorizedAccessException("You are not the owner of this post.");

            var oldTags = post.PostTags.Select(pt => pt.Tag).ToList();

            bool captionChanged = false;
            if (!string.IsNullOrWhiteSpace(dto.Caption) && dto.Caption != post.Caption)
            {
                post.Caption = dto.Caption;
                captionChanged = true;
            }

            if (dto.Visibility != null)
                post.Visibility = dto.Visibility.Value;

            if (dto.Image != null && dto.Image.Length > 0)
            {
                if (!string.IsNullOrEmpty(post.ImageUrl))
                    await _blobService.DeleteAsync(post.ImageUrl, BlobContainers.Post);

                post.ImageUrl = await _blobService.UploadAsync(dto.Image, BlobContainers.Post);
            }

            if (captionChanged)
            {
                var newTagNames = ExtractHashtags(post.Caption ?? "");
                var newTagSet = new HashSet<string>(newTagNames);
                var oldTagSet = new HashSet<string>(oldTags.Select(t => t.Name));

                foreach (var old in oldTags)
                {
                    if (!newTagSet.Contains(old.Name))
                    {
                        var link = post.PostTags.FirstOrDefault(pt => pt.TagId == old.Id);
                        if (link != null)
                            _context.PostTags.Remove(link);
                    }
                }

                var existingTags = await _context.Tags
                    .Where(t => newTagNames.Contains(t.Name))
                    .ToDictionaryAsync(t => t.Name);

                foreach (var tagName in newTagNames)
                {
                    if (!oldTagSet.Contains(tagName))
                    {
                        if (!existingTags.TryGetValue(tagName, out var tag))
                        {
                            tag = new Tag { Id = Guid.NewGuid(), Name = tagName };
                            _context.Tags.Add(tag);
                        }

                        _context.PostTags.Add(new PostTag
                        {
                            PostId = post.Id,
                            TagId = tag.Id
                        });

                        await _elastic.IndexTagAsync(new TagIndex
                        {
                            Id = tag.Id,
                            Name = tag.Name
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            await _elastic.IndexPostAsync(new PostIndex
            {
                Id = post.Id,
                Caption = post.Caption ?? string.Empty,
                UserId = post.User.Id,
                Username = post.User.Username,
                CreatedAt = post.CreatedAt,
                Visibility = post.Visibility.ToString()
            });

            return post;
        }

        public async Task<PagedResult<PostListItemDto>> GetAllPublicPostsAsync(Guid? currentUserId, int page, int pageSize)
        {
            var query = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Where(p => p.Visibility == PostVisibility.Public)
                .OrderByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync();

            var posts = await query
                .Skip((page - 1) * pageSize)
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
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = posts
            };
        }

        public async Task<PostListItemDto> GetPostByIdAsync(Guid postId, Guid? currentUserId)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                throw new KeyNotFoundException("Post not found.");

            if (post.Visibility == PostVisibility.Private && post.UserId != currentUserId)
                throw new UnauthorizedAccessException("You cannot view this private post.");

            if (post.Visibility == PostVisibility.FollowersOnly && post.UserId != currentUserId)
            {
                if (currentUserId == null)
                    throw new UnauthorizedAccessException("Login required to view this post.");

                var isFollower = await _context.UserFollows.AnyAsync(f =>
                    f.FollowingId == post.UserId && f.FollowerId == currentUserId);

                if (!isFollower)
                    throw new UnauthorizedAccessException("You must be a follower to view this post.");
            }

            return new PostListItemDto
            {
                Id = post.Id,
                Caption = post.Caption,
                ImageUrl = post.ImageUrl,
                CreatedAt = post.CreatedAt,
                Likes = post.Likes.Count,
                Liked = currentUserId != null && post.Likes.Any(l => l.UserId == currentUserId),
                User = new SimpleUserDto
                {
                    Id = post.User.Id,
                    Username = post.User.Username,
                    ProfileImageUrl = post.User.ProfileImageUrl
                }
            };
        }

        public async Task DeletePostAsync(Guid postId, Guid userId)
        {
            var post = await _context.Posts
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                throw new KeyNotFoundException("Post not found.");

            if (post.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to delete this post.");

            // Remove associated PostTags
            _context.PostTags.RemoveRange(post.PostTags);

            // Remove post
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            // Delete from blob storage
            if (!string.IsNullOrEmpty(post.ImageUrl))
                await _blobService.DeleteAsync(post.ImageUrl, BlobContainers.Post);

            // Delete from Elasticsearch
            await _elastic.DeletePostAsync(post.Id);
        }

        public async Task LikePostAsync(Guid postId, Guid userId)
        {
            var alreadyLiked = await _context.PostLikes
                .AnyAsync(l => l.PostId == postId && l.UserId == userId);

            if (alreadyLiked)
                throw new InvalidOperationException("You have already liked this post.");

            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                throw new KeyNotFoundException("Post not found.");

            _context.PostLikes.Add(new PostLike
            {
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });

            if (post.UserId != userId)
            {
                var fromUser = await _context.Users.FindAsync(userId);
                if (fromUser != null)
                {
                    _context.Notifications.Add(new Notification
                    {
                        RecipientId = post.UserId,
                        FromUserId = userId,
                        Type = NotificationType.Like,
                        PostId = post.Id,
                        Message = $"{fromUser.Username} liked your post.",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task UnlikePostAsync(Guid postId, Guid userId)
        {
            var like = await _context.PostLikes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (like == null)
                throw new KeyNotFoundException("Like not found.");

            _context.PostLikes.Remove(like);

            var post = await _context.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post != null && post.UserId != userId)
            {
                var notification = await _context.Notifications.FirstOrDefaultAsync(n =>
                    n.Type == NotificationType.Like &&
                    n.PostId == postId &&
                    n.FromUserId == userId &&
                    n.RecipientId == post.UserId);

                if (notification != null)
                    _context.Notifications.Remove(notification);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<PostListItemDto>> GetUserFeedAsync(Guid userId, int page, int pageSize)
        {
            var followingIds = await _context.UserFollows
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FollowingId)
                .ToListAsync();

            var query = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Where(p => followingIds.Contains(p.UserId))
                .OrderByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync();

            var posts = await query
                .Skip((page - 1) * pageSize)
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
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = posts
            };
        }

        public async Task<PagedResult<SimpleUserDto>> GetPostLikesAsync(Guid postId, int page, int pageSize)
        {
            var query = _context.PostLikes
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

        public async Task<List<PostListItemDto>> GetExplorePostsAsync(Guid? userId)
        {
            if (userId == null)
            {
                return await _postRepository.GetPopularPostsAsync();
            }

            var recommended = await _recommendClient.GetRecommendationsAsync(userId.Value);

            if (recommended.Count > 0)
                return recommended;

            return await _postRepository.GetPopularPostsNotFollowedAsync(userId.Value);
        }
    }
}
