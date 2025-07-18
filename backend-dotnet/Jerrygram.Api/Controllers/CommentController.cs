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
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var currentUserId = Guid.Parse(userId);

            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                return NotFound(new { error = "Post not found." });

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (user == null)
                return Unauthorized();

            var comment = new Comment
            {
                PostId = postId,
                UserId = currentUserId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);

            // Save notification if commenter is not the post owner
            if (post.UserId != currentUserId)
            {
                _context.Notifications.Add(new Notification
                {
                    RecipientId = post.UserId,
                    FromUserId = currentUserId,
                    Type = NotificationType.Comment,
                    PostId = postId,
                    Message = $"{user.Username} commented on your post.",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            var response = new CommentResponseDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                User = new SimpleUserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    ProfileImageUrl = user.ProfileImageUrl
                }
            };

            return CreatedAtAction(nameof(GetComments), new { postId }, response);
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

            var currentUserId = Guid.Parse(userId);

            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
                return NotFound();

            if (comment.UserId != currentUserId)
                return Forbid();

            _context.Comments.Remove(comment);

            var notification = await _context.Notifications.FirstOrDefaultAsync(n =>
                n.Type == NotificationType.Comment &&
                n.PostId == comment.PostId &&
                n.FromUserId == currentUserId);

            if (notification != null)
                _context.Notifications.Remove(notification);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
