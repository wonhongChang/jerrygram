using Jerrygram.Api.Data;
using Jerrygram.Api.Models;
using Jerrygram.Api.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Jerrygram.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ElasticService _elastic;
        private readonly AppDbContext _context;

        public SearchController(ElasticService elastic, AppDbContext context)
        {
            _elastic = elastic;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { error = "Query is required." });

            var users = await _elastic.SearchUsersAsync(query);
            var posts = await _elastic.SearchPostsAsync(query);

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = string.IsNullOrEmpty(userIdStr) ? (Guid?)null : Guid.Parse(userIdStr);

            List<Guid> followingIds = [];
            if (userId != null)
            {
                followingIds = await _context.UserFollows
                    .Where(f => f.FollowerId == userId)
                    .Select(f => f.FollowingId)
                    .ToListAsync();
            }

            var filteredPosts = posts.Where(p =>
                p.Visibility == PostVisibility.Public.ToString() ||
                (p.Visibility == PostVisibility.FollowersOnly.ToString() && userId != null && followingIds.Contains(p.UserId))
            );

            return Ok(new
            {
                query,
                suggestions = new
                {
                    users = users.Select(u => new
                    {
                        u.Id,
                        u.Username,
                        u.ProfileImageUrl
                    }),
                    posts = filteredPosts.Select(p => new
                    {
                        p.Id,
                        p.Caption,
                        p.Username,
                        p.CreatedAt
                    })
                }
            });
        }
    }
}
