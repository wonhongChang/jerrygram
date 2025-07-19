using Jerrygram.Api.Data;
using Jerrygram.Api.Dtos;
using Jerrygram.Api.Models;
using Jerrygram.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Jerrygram.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var user = await _context.Users
                .Where(u => u.Id == Guid.Parse(userId))
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.ProfileImageUrl,
                    u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Get a public user profile by username.
        /// </summary>
        /// <param name="username">The username of the target user.</param>
        /// <returns>User profile data with follower/following counts.</returns>
        [AllowAnonymous]
        [HttpGet("{username}")]
        public async Task<IActionResult> GetProfile(string username)
        {
            var user = await _context.Users
                .Where(u => u.Username.ToLower() == username.ToLower())
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.ProfileImageUrl,
                    u.CreatedAt,
                    Followers = u.Followers.Count,
                    Followings = u.Followings.Count
                })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();
            return Ok(user);
        }

        /// <summary>
        /// Follow a user by their ID. Requires authentication.
        /// </summary>
        /// <param name="id">Target user ID to follow.</param>
        /// <returns>200 OK on success, 400 if invalid or already following.</returns>
        [HttpPost("{id}/follow")]
        public async Task<IActionResult> FollowUser(Guid id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null || currentUserId == id.ToString())
                return BadRequest();

            var targetExists = await _context.Users.AnyAsync(u => u.Id == id);
            if (!targetExists)
                return NotFound(new { error = "User not found." });

            var currentUserGuid = Guid.Parse(currentUserId);

            var exists = await _context.UserFollows.AnyAsync(f =>
                f.FollowerId == currentUserGuid && f.FollowingId == id);

            if (exists)
                return BadRequest(new { message = "Already following." });

            var follow = new UserFollow
            {
                FollowerId = currentUserGuid,
                FollowingId = id,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserFollows.Add(follow);

            if (id != currentUserGuid)
            {
                var user = await _context.Users.FindAsync(currentUserGuid);
                if (user != null)
                {
                    _context.Notifications.Add(new Notification
                    {
                        RecipientId = id,
                        FromUserId = currentUserGuid,
                        Type = NotificationType.Follow,
                        Message = $"{user.Username} started following you.",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Unfollow a user by their ID. Requires authentication.
        /// </summary>
        /// <param name="id">Target user ID to unfollow.</param>
        /// <returns>200 OK on success, 404 if no existing follow found.</returns>
        [HttpDelete("{id}/unfollow")]
        public async Task<IActionResult> UnfollowUser(Guid id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null || currentUserId == id.ToString())
                return BadRequest();

            var currentUserGuid = Guid.Parse(currentUserId);

            var targetExists = await _context.Users.AnyAsync(u => u.Id == id);
            if (!targetExists)
                return NotFound(new { error = "User not found." });

            var follow = await _context.UserFollows.FirstOrDefaultAsync(f =>
                f.FollowerId == Guid.Parse(currentUserId) && f.FollowingId == id);

            if (follow == null)
                return NotFound();

            _context.UserFollows.Remove(follow);

            var notification = await _context.Notifications.FirstOrDefaultAsync(n =>
                n.Type == NotificationType.Follow &&
                n.FromUserId == currentUserGuid &&
                n.RecipientId == id);

            if (notification != null)
                _context.Notifications.Remove(notification);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Get a list of users who follow the specified user.
        /// </summary>
        /// <param name="id">The ID of the user whose followers will be returned.</param>
        /// <returns>List of follower user profiles.</returns>
        [AllowAnonymous]
        [HttpGet("{id}/followers")]
        public async Task<IActionResult> GetFollowers(Guid id)
        {
            var exists = await _context.Users.AnyAsync(u => u.Id == id);
            if (!exists)
                return NotFound(new { error = "User not found." });

            var followers = await _context.UserFollows
                .Where(f => f.FollowingId == id)
                .Include(f => f.Follower)
                .Select(f => new
                {
                    f.Follower.Id,
                    f.Follower.Username,
                    f.Follower.ProfileImageUrl
                })
                .ToListAsync();

            return Ok(followers);
        }

        /// <summary>
        /// Get a list of users that the specified user is following.
        /// </summary>
        /// <param name="id">The ID of the user whose followings will be returned.</param>
        /// <returns>List of following user profiles.</returns>
        [AllowAnonymous]
        [HttpGet("{id}/followings")]
        public async Task<IActionResult> GetFollowings(Guid id)
        {
            var exists = await _context.Users.AnyAsync(u => u.Id == id);
            if (!exists)
                return NotFound(new { error = "User not found." });

            var followings = await _context.UserFollows
                .Where(f => f.FollowerId == id)
                .Include(f => f.Following)
                .Select(f => new
                {
                    f.Following.Id,
                    f.Following.Username,
                    f.Following.ProfileImageUrl
                })
                .ToListAsync();

            return Ok(followings);
        }

        /// <summary>
        /// Upload or update your profile avatar.
        /// </summary>
        /// <param name="dto">Multipart/form-data containing image file.</param>
        /// <param name="blobService">Injected Azure Blob storage service</param>
        /// <returns>Returns uploaded image URL</returns>
        [HttpPost("me/avatar")]
        public async Task<IActionResult> UploadAvatar([FromForm] AvatarUploadDto dto, [FromServices] BlobService blobService)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _context.Users.FindAsync(Guid.Parse(userId));
            if (user == null) return NotFound();

            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                await blobService.DeleteAsync(user.ProfileImageUrl, "ProfileContainer");
            }

            var imageUrl = await blobService.UploadAsync(dto.Avatar, "ProfileContainer");
            user.ProfileImageUrl = imageUrl;

            await _context.SaveChangesAsync();

            return Ok(new { imageUrl });
        }
    }
}
