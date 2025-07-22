using Jerrygram.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Jerrygram.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExploreController : ControllerBase
    {
        private readonly IPostService _postService;

        public ExploreController(IPostService postService)
        {
            _postService = postService;
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

            var posts = await _postService.GetExplorePostsAsync(userId);
            return Ok(posts); ;
        }
    }
}
