using Jerrygram.Api.Data;
using Jerrygram.Api.Dtos;
using Jerrygram.Api.Interfaces;
using Jerrygram.Api.Search;
using Jerrygram.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ElasticService _elastic;
        private readonly IPostService _postService;

        public PostController(AppDbContext context, BlobService blobService, ILogger<PostController> logger, ElasticService elastic, IPostService postService)
        {
            _context = context;
            _blobService = blobService;
            _logger = logger;
            _elastic = elastic;
            _postService = postService;
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
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            try
            {
                var post = await _postService.CreatePostAsync(dto, userId);
                return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, post);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
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
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            try
            {
                var updated = await _postService.UpdatePostAsync(id, dto, userId);

                return Ok(new
                {
                    updated.Id,
                    updated.Caption,
                    updated.ImageUrl,
                    updated.CreatedAt,
                    updated.Visibility
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
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

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Guid.TryParse(userIdStr, out var parsed) ? parsed : (Guid?)null;

            var result = await _postService.GetAllPublicPostsAsync(userId, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Get a specific post by ID.
        /// </summary>
        /// <param name="id">Post ID</param>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Guid.TryParse(userIdStr, out var parsed) ? parsed : (Guid?)null;

            try
            {
                var post = await _postService.GetPostByIdAsync(id, userId);
                return Ok(post);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Delete a post by ID (only by the creator).
        /// </summary>
        /// <param name="id">Post ID</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("User not authenticated.");

            try
            {
                await _postService.DeletePostAsync(id, userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
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
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            try
            {
                await _postService.LikePostAsync(id, userId);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Post not found.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Unlike a post.
        /// </summary>
        /// <param name="id">The ID of the post to unlike.</param>
        /// <returns>200 OK on success or 404 if like not found.</returns>
        [HttpDelete("{id}/like")]
        public async Task<IActionResult> UnlikePost(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            try
            {
                await _postService.UnlikePostAsync(id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Like not found.");
            }
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

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var result = await _postService.GetUserFeedAsync(userId, page, pageSize);
            return Ok(result);
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

            var result = await _postService.GetPostLikesAsync(id, page, pageSize);
            return Ok(result);
        }
    }
}
