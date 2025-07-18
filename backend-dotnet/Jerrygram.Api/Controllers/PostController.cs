using Jerrygram.Api.Data;
using Jerrygram.Api.Dtos;
using Jerrygram.Api.Models;
using Jerrygram.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Jerrygram.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly BlobService _blobService;
        private readonly ILogger<PostController> _logger;

        public PostController(AppDbContext context, BlobService blobService, ILogger<PostController> logger)
        {
            _context = context;
            _blobService = blobService;
            _logger = logger;
        }

        /// <summary>
        /// Upload a new post with image and caption.
        /// </summary>
        /// <param name="caption">The caption text.</param>
        /// <param name="image">The image file to upload.</param>
        /// <returns>201 Created with post data.</returns>
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] PostUploadDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(dto.Caption) && (dto.Image == null || dto.Image.Length == 0))
                return BadRequest("At least one of caption or image must be provided.");

            string? imageUrl = null;
            if (dto.Image != null && dto.Image.Length > 0)
            {
                imageUrl = await _blobService.UploadAsync(dto.Image);
            }

            var post = new Post
            {
                Caption = dto.Caption,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow,
                UserId = Guid.Parse(userId)
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, post);
        }

        /// <summary>
        /// Update the caption and/or image of an existing post.
        /// </summary>
        /// <param name="id">The ID of the post to update.</param>
        /// <param name="dto">The new caption and/or image file to apply.</param>
        /// <returns>200 OK with updated post data.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(Guid id, [FromForm] UpdatePostDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
                return NotFound();

            if (post.UserId != Guid.Parse(userId))
                return Forbid();

            // Update caption if provided
            if (!string.IsNullOrWhiteSpace(dto.Caption))
                post.Caption = dto.Caption;

            // Replace image if a new one is provided
            if (dto.Image != null && dto.Image.Length > 0)
            {
                var newImageUrl = await _blobService.UploadAsync(dto.Image);
                post.ImageUrl = newImageUrl;
            }

            await _context.SaveChangesAsync();

            return Ok(post);
        }

        /// <summary>
        /// Get all posts in descending order.
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
                return BadRequest("Invalid pagination parameters.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserId = string.IsNullOrEmpty(userId) ? Guid.Empty : Guid.Parse(userId);

            var query = _context.Posts
                .Include(p => p.User)
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
                    Liked = p.Likes.Any(l => l.UserId == currentUserId),
                    User = new SimpleUserDto
                    {
                        Id = p.User.Id,
                        Username = p.User.Username,
                        ProfileImageUrl = p.User.ProfileImageUrl
                    }
                })
                .ToListAsync();

            return Ok(new PagedResult<PostListItemDto>
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = posts
            });
        }

        /// <summary>
        /// Get a specific post by ID.
        /// </summary>
        /// <param name="id">Post ID</param>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserId = string.IsNullOrEmpty(userId) ? Guid.Empty : Guid.Parse(userId);

            var post = await _context.Posts
                .Include(p => p.User)
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Caption,
                    p.ImageUrl,
                    p.CreatedAt,
                    User = new
                    {
                        p.User.Id,
                        p.User.Username,
                        p.User.ProfileImageUrl
                    },
                    Likes = p.Likes.Count,
                    Liked = p.Likes.Any(l => l.UserId == currentUserId)
                })
                .FirstOrDefaultAsync();

            if (post == null) return NotFound();
            return Ok(post);
        }

        /// <summary>
        /// Delete a post by ID (only by the creator).
        /// </summary>
        /// <param name="id">Post ID</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

                var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
                if (post == null)
                    return NotFound();

                if (post.UserId != Guid.Parse(userId))
                    return Forbid();

                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post with ID {PostId}", id);
                return StatusCode(500, new { error = "An unexpected error occurred while deleting the post." });
            }
        }

        /// <summary>
        /// Like a post.
        /// </summary>
        /// <param name="id">The ID of the post to like.</param>
        /// <returns>200 OK on success or 400 if already liked.</returns>
        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikePost(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var currentUserId = Guid.Parse(userId);

            var alreadyLiked = await _context.PostLikes
                .AnyAsync(l => l.PostId == id && l.UserId == Guid.Parse(userId));

            if (alreadyLiked)
                return BadRequest("You have already liked this post.");

            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound("Post not found.");

            var like = new PostLike
            {
                PostId = id,
                UserId = Guid.Parse(userId),
                CreatedAt = DateTime.UtcNow
            };

            _context.PostLikes.Add(like);

            if (post.UserId != currentUserId)
            {
                var user = await _context.Users.FindAsync(currentUserId);
                if (user != null)
                {
                    _context.Notifications.Add(new Notification
                    {
                        RecipientId = post.UserId,
                        FromUserId = currentUserId,
                        Type = NotificationType.Like,
                        PostId = id,
                        Message = $"{user.Username} liked your post.",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Unlike a post.
        /// </summary>
        /// <param name="id">The ID of the post to unlike.</param>
        /// <returns>200 OK on success or 404 if like not found.</returns>
        [HttpDelete("{id}/like")]
        public async Task<IActionResult> UnlikePost(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var currentUserId = Guid.Parse(userId);

            var like = await _context.PostLikes
                .FirstOrDefaultAsync(l => l.PostId == id && l.UserId == Guid.Parse(userId));

            if (like == null)
                return NotFound();

            _context.PostLikes.Remove(like);

            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (post != null && post.UserId != currentUserId)
            {
                var notification = await _context.Notifications.FirstOrDefaultAsync(n =>
                    n.Type == NotificationType.Like &&
                    n.PostId == id &&
                    n.FromUserId == currentUserId &&
                    n.RecipientId == post.UserId);

                if (notification != null)
                    _context.Notifications.Remove(notification);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Get feed posts from followed users.
        /// </summary>
        /// <returns>A list of posts from followed users.</returns>
        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
                return BadRequest("Invalid pagination parameters.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var currentUserId = Guid.Parse(userId);

            var followingIds = await _context.UserFollows
                .Where(f => f.FollowerId == currentUserId)
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
                    Liked = p.Likes.Any(l => l.UserId == currentUserId),
                    User = new SimpleUserDto
                    {
                        Id = p.User.Id,
                        Username = p.User.Username,
                        ProfileImageUrl = p.User.ProfileImageUrl
                    }
                })
                .ToListAsync();

            return Ok(new PagedResult<PostListItemDto>
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = posts
            });
        }

        /// <summary>
        /// Get a list of users who liked the post.
        /// </summary>
        /// <param name="id">The ID of the post.</param>
        /// <returns>List of users who liked the post.</returns>
        [AllowAnonymous]
        [HttpGet("{id}/likes")]
        public async Task<IActionResult> GetPostLikes(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
                return BadRequest("Invalid pagination parameters.");

            var query = _context.PostLikes
                .Where(l => l.PostId == id)
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

            return Ok(new PagedResult<SimpleUserDto>
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = users
            });
        }
    }
}
