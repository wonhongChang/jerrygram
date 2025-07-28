using Application.Commands;
using Application.Commands.Posts;
using Application.DTOs;
using Application.Queries;
using Application.Queries.Posts;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        private readonly ICommandHandler<DeletePostCommand> _deletePostHandler;
        private readonly IQueryHandler<GetPostByIdQuery, PostListItemDto> _getPostByIdHandler;
        private readonly IQueryHandler<GetPublicPostsQuery, PagedResult<PostListItemDto>> _getPublicPostsHandler;
        private readonly IQueryHandler<GetUserFeedQuery, PagedResult<PostListItemDto>> _getUserFeedHandler;
        private readonly ILogger<PostController> _logger;

        public PostController(
            ICommandHandler<CreatePostCommand, PostListItemDto> createPostHandler,
            ICommandHandler<UpdatePostCommand, Post> updatePostHandler,
            ICommandHandler<LikePostCommand> likePostHandler,
            ICommandHandler<DeletePostCommand> deletePostHandler,
            IQueryHandler<GetPostByIdQuery, PostListItemDto> getPostByIdHandler,
            IQueryHandler<GetPublicPostsQuery, PagedResult<PostListItemDto>> getPublicPostsHandler,
            IQueryHandler<GetUserFeedQuery, PagedResult<PostListItemDto>> getUserFeedHandler,
            ILogger<PostController> logger)
        {
            _createPostHandler = createPostHandler;
            _updatePostHandler = updatePostHandler;
            _likePostHandler = likePostHandler;
            _deletePostHandler = deletePostHandler;
            _getPostByIdHandler = getPostByIdHandler;
            _getPublicPostsHandler = getPublicPostsHandler;
            _getUserFeedHandler = getUserFeedHandler;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] PostUploadDto dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var command = new CreatePostCommand { Dto = dto, UserId = userId.Value };
            var result = await _createPostHandler.HandleAsync(command);
            
            return CreatedAtAction(nameof(GetPostById), new { id = result.Id }, result);
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

            var command = new LikePostCommand { PostId = id, UserId = userId.Value };
            await _likePostHandler.HandleAsync(command);
            
            return Ok();
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

            var command = new DeletePostCommand { PostId = id, UserId = userId.Value };
            await _deletePostHandler.HandleAsync(command);
            
            return NoContent();
        }

        private Guid? GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out var userId) ? userId : null;
        }
    }
}