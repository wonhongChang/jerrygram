using Application.DTOs;
using Application.Queries;
using Application.Queries.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExploreController : ControllerBase
    {
        private readonly IQueryHandler<GetExplorePostsQuery, List<PostListItemDto>> _getExplorePostsHandler;

        public ExploreController(IQueryHandler<GetExplorePostsQuery, List<PostListItemDto>> getExplorePostsHandler)
        {
            _getExplorePostsHandler = getExplorePostsHandler;
        }

        /// <summary>
        /// Get AI-recommended or popular posts for the Explore feed.
        /// </summary>
        /// <remarks>
        /// - If the user is logged in, returns AI-based recommended posts.
        /// - If not logged in or no recommendation is available, returns public popular posts.
        /// </remarks>
        /// <response code="200">Returns a list of post items</response>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetExploreFeed()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Guid? userId = null;
            if (Guid.TryParse(userIdStr, out var parsed))
                userId = parsed;

            var query = new GetExplorePostsQuery { UserId = userId };
            var posts = await _getExplorePostsHandler.HandleAsync(query);
            return Ok(posts);
        }
    }
}