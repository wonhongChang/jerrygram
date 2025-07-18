using Jerrygram.Api.Data;
using Jerrygram.Api.Dtos;
using Jerrygram.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Jerrygram.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CommentController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create a comment on a specific post.
        /// </summary>
        [HttpPost("/api/posts/{postId}/comments")]
        public async Task<IActionResult> CreateComment(Guid postId, [FromBody] CreateCommentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists)
                return NotFound(new { error = "Post not found." });

            var comment = new Comment
            {
                PostId = postId,
                UserId = Guid.Parse(userId),
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetComments), new { postId = postId }, comment);
        }

        /// <summary>
        /// Get comments for a specific post.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("/api/posts/{postId}/comments")]
        public async Task<IActionResult> GetComments(Guid postId)
        {
            var comments = await _context.Comments
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

            return Ok(comments);
        }

        /// <summary>
        /// Delete a comment (only by the comment owner).
        /// </summary>
        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
                return NotFound();

            if (comment.UserId != Guid.Parse(userId))
                return Forbid();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
