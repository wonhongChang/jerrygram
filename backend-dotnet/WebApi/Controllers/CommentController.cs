using Application.Commands;
using Application.Commands.Comments;
using Application.DTOs;
using Application.Queries;
using Application.Queries.Comments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommandHandler<CreateCommentCommand, CommentResponseDto> _createCommentHandler;
        private readonly ICommandHandler<DeleteCommentCommand> _deleteCommentHandler;
        private readonly IQueryHandler<GetCommentsQuery, object> _getCommentsHandler;

        public CommentController(
            ICommandHandler<CreateCommentCommand, CommentResponseDto> createCommentHandler,
            ICommandHandler<DeleteCommentCommand> deleteCommentHandler,
            IQueryHandler<GetCommentsQuery, object> getCommentsHandler)
        {
            _createCommentHandler = createCommentHandler;
            _deleteCommentHandler = deleteCommentHandler;
            _getCommentsHandler = getCommentsHandler;
        }

        /// <summary>
        /// Create a comment on a specific post.
        /// </summary>
        [HttpPost("/api/posts/{postId}/comments")]
        public async Task<IActionResult> CreateComment(Guid postId, [FromBody] CreateCommentDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            try
            {
                var command = new CreateCommentCommand
                {
                    PostId = postId,
                    UserId = userId,
                    Dto = dto
                };

                var result = await _createCommentHandler.HandleAsync(command);
                return CreatedAtAction(nameof(GetComments), new { postId }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get comments for a specific post.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("/api/posts/{postId}/comments")]
        public async Task<IActionResult> GetComments(Guid postId)
        {
            var query = new GetCommentsQuery { PostId = postId };
            var result = await _getCommentsHandler.HandleAsync(query);
            return Ok(result);
        }

        /// <summary>
        /// Delete a comment (only by the comment owner).
        /// </summary>
        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            try
            {
                var command = new DeleteCommentCommand
                {
                    CommentId = commentId,
                    UserId = userId
                };

                await _deleteCommentHandler.HandleAsync(command);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}