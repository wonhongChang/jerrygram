using Application.Commands;
using Application.Commands.Posts;
using Application.DTOs;
using Application.Events;
using Application.Interfaces;
using Application.Queries;
using Application.Queries.Posts;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.Extensions;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/posts")]
    public class PostController : ControllerBase
    {
        private readonly ICommandHandler<CreatePostCommand, PostListItemDto> _createPostHandler;
        private readonly ICommandHandler<UpdatePostCommand, Post> _updatePostHandler;
        private readonly ICommandHandler<LikePostCommand> _likePostHandler;
        private readonly ICommandHandler<UnlikePostCommand> _unlikePostHandler;
        private readonly ICommandHandler<DeletePostCommand> _deletePostHandler;
        private readonly IQueryHandler<GetPostByIdQuery, PostListItemDto> _getPostByIdHandler;
        private readonly IQueryHandler<GetPublicPostsQuery, PagedResult<PostListItemDto>> _getPublicPostsHandler;
        private readonly IQueryHandler<GetUserFeedQuery, PagedResult<PostListItemDto>> _getUserFeedHandler;
        private readonly ILogger<PostController> _logger;
        private readonly IEventService _eventService;

        public PostController(
            ICommandHandler<CreatePostCommand, PostListItemDto> createPostHandler,
            ICommandHandler<UpdatePostCommand, Post> updatePostHandler,
            ICommandHandler<LikePostCommand> likePostHandler,
            ICommandHandler<UnlikePostCommand> unlikePostHandler,
            ICommandHandler<DeletePostCommand> deletePostHandler,
            IQueryHandler<GetPostByIdQuery, PostListItemDto> getPostByIdHandler,
            IQueryHandler<GetPublicPostsQuery, PagedResult<PostListItemDto>> getPublicPostsHandler,
            IQueryHandler<GetUserFeedQuery, PagedResult<PostListItemDto>> getUserFeedHandler,
            ILogger<PostController> logger,
            IEventService eventService)
        {
            _createPostHandler = createPostHandler;
            _updatePostHandler = updatePostHandler;
            _likePostHandler = likePostHandler;
            _unlikePostHandler = unlikePostHandler;
            _deletePostHandler = deletePostHandler;
            _getPostByIdHandler = getPostByIdHandler;
            _getPublicPostsHandler = getPublicPostsHandler;
            _getUserFeedHandler = getUserFeedHandler;
            _logger = logger;
            _eventService = eventService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] PostUploadDto dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            try
            {
                var command = new CreatePostCommand { Dto = dto, UserId = userId.Value };
                var result = await _createPostHandler.HandleAsync(command);

                var postEvent = new PostEvent
                {
                    PostId = result.Id.ToString(),
                    EventType = "create",
                    Content = dto.Caption?.Substring(0, Math.Min(dto.Caption?.Length ?? 0, 100)),
                    Metadata = new Dictionary<string, object>
                    {
                        { "hasImage", dto.Image != null },
                        { "captionLength", dto.Caption?.Length ?? 0 },
                        { "visibility", dto.Visibility.ToString() }
                    }
                };

                HttpContext.EnrichEvent(postEvent);
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _eventService.PublishPostEventAsync(postEvent);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to publish post creation event for post: {PostId}", result.Id);
                    }
                });

                return CreatedAtAction(nameof(GetPostById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post for user {UserId}", userId);
                return StatusCode(500, "An error occurred while creating the post.");
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
                return BadRequest("Invalid pagination parameters.");

            var query = new GetPublicPostsQuery 
            { 
                CurrentUserId = GetCurrentUserId(),
                Page = page,
                PageSize = pageSize 
            };
            
            var result = await _getPublicPostsHandler.HandleAsync(query);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(Guid id)
        {
            var query = new GetPostByIdQuery 
            { 
                PostId = id,
                CurrentUserId = GetCurrentUserId()
            };
            
            var result = await _getPostByIdHandler.HandleAsync(query);
            return Ok(result);
        }

        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikePost(Guid id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            try
            {
                var command = new LikePostCommand { PostId = id, UserId = userId.Value };
                await _likePostHandler.HandleAsync(command);

                var likeEvent = new PostEvent
                {
                    PostId = id.ToString(),
                    EventType = "like",
                    Metadata = new Dictionary<string, object>
                    {
                        { "action", "add" }
                    }
                };

                HttpContext.EnrichEvent(likeEvent);
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _eventService.PublishPostEventAsync(likeEvent);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to publish like event for post: {PostId}", id);
                    }
                });

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling like for post: {PostId}", id);
                return StatusCode(500, "An error occurred while processing the like");
            }
        }

        [HttpDelete("{id}/like")]
        public async Task<IActionResult> UnlikePost(Guid id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            try
            {
                var command = new UnlikePostCommand { PostId = id, UserId = userId.Value };
                await _unlikePostHandler.HandleAsync(command);

                var unlikeEvent = new PostEvent
                {
                    PostId = id.ToString(),
                    EventType = "unlike",
                    Metadata = new Dictionary<string, object>
            {
                { "action", "remove" }
            }
                };

                HttpContext.EnrichEvent(unlikeEvent);
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _eventService.PublishPostEventAsync(unlikeEvent);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to publish unlike event for post: {PostId}", id);
                    }
                });

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unliking post: {PostId}", id);
                return StatusCode(500, "An error occurred while processing the unlike");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(Guid id, [FromBody] UpdatePostDto dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var command = new UpdatePostCommand { PostId = id, Dto = dto, UserId = userId.Value };
            var result = await _updatePostHandler.HandleAsync(command);
            
            return Ok(result);
        }

        [HttpGet("feed")]
        public async Task<IActionResult> GetUserFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            if (page < 1 || pageSize < 1)
                return BadRequest("Invalid pagination parameters.");

            var query = new GetUserFeedQuery 
            { 
                UserId = userId.Value,
                Page = page,
                PageSize = pageSize 
            };
            
            var result = await _getUserFeedHandler.HandleAsync(query);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            try
            {
                var command = new DeletePostCommand { PostId = id, UserId = userId.Value };
                await _deletePostHandler.HandleAsync(command);

                var deleteEvent = new PostEvent
                {
                    PostId = id.ToString(),
                    EventType = "delete"
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
                        _logger.LogWarning(ex, "Failed to publish delete event for post: {PostId}", id);
                    }
                });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post: {PostId}", id);
                return StatusCode(500, "An error occurred while deleting the post");
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out var userId) ? userId : null;
        }
    }
}