using Application.Commands;
using Application.Commands.Comments;
using Application.DTOs;
using Application.Events;
using Application.Interfaces;
using Application.Queries;
using Application.Queries.Comments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.Extensions;

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

        private readonly IEventService _eventService;
        private readonly ILogger<CommentController> _logger;

        public CommentController(
            ICommandHandler<CreateCommentCommand, CommentResponseDto> createCommentHandler,
            ICommandHandler<DeleteCommentCommand> deleteCommentHandler,
            IQueryHandler<GetCommentsQuery, object> getCommentsHandler,
            IEventService eventService,
            ILogger<CommentController> logger)
        {
            _createCommentHandler = createCommentHandler;
            _deleteCommentHandler = deleteCommentHandler;
            _getCommentsHandler = getCommentsHandler;
            _eventService = eventService;
            _logger = logger;
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

                var commentEvent = new PostEvent
                {
                    PostId = postId.ToString(),
                    EventType = "comment",
                    Content = dto.Content?[..Math.Min(dto.Content?.Length ?? 0, 100)],
                    Metadata = new Dictionary<string, object>
                {
                    { "commentId", result.Id },
                    { "contentLength", dto.Content?.Length ?? 0 }
                }
                };

                HttpContext.EnrichEvent(commentEvent);
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _eventService.PublishPostEventAsync(commentEvent);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to publish comment event for post: {PostId}", postId);
                    }
                });

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment for post: {PostId}", postId);
                return StatusCode(500, "An error occurred while creating the comment");
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

                var deleteEvent = new PostEvent
                {
                    PostId = "unknown",
                    EventType = "comment_delete",
                    Metadata = new Dictionary<string, object>
                {
                    { "commentId", commentId.ToString() }
                }
                };

                HttpContext.EnrichEvent(deleteEvent);
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _eventService.PublishPostEventAsync(deleteEvent);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to publish comment delete event for comment: {CommentId}", commentId);
                    }
                });

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment: {CommentId}", commentId);
                return StatusCode(500, "An error occurred while deleting the comment");
            }
        }
    }
}