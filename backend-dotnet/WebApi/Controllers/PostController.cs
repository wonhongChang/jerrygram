using Application.Commands;
using Application.Commands.Posts;
using Application.DTOs;
using Application.Queries;
using Application.Queries.Posts;
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
        private readonly ICommandHandler<LikePostCommand> _likePostHandler;
        private readonly ICommandHandler<DeletePostCommand> _deletePostHandler;
        private readonly IQueryHandler<GetPostByIdQuery, PostListItemDto> _getPostByIdHandler;
        private readonly IQueryHandler<GetPublicPostsQuery, PagedResult<PostListItemDto>> _getPublicPostsHandler;
        private readonly ILogger<PostController> _logger;

        public PostController(
            ICommandHandler<CreatePostCommand, PostListItemDto> createPostHandler,
            ICommandHandler<LikePostCommand> likePostHandler,
            ICommandHandler<DeletePostCommand> deletePostHandler,
            IQueryHandler<GetPostByIdQuery, PostListItemDto> getPostByIdHandler,
            IQueryHandler<GetPublicPostsQuery, PagedResult<PostListItemDto>> getPublicPostsHandler,
            ILogger<PostController> logger)
        {
            _createPostHandler = createPostHandler;
            _likePostHandler = likePostHandler;
            _deletePostHandler = deletePostHandler;
            _getPostByIdHandler = getPostByIdHandler;
            _getPublicPostsHandler = getPublicPostsHandler;
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