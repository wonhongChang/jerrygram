using Application.Commands;
using Application.Commands.Users;
using Application.DTOs;
using Application.Queries;
using Application.Queries.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IQueryHandler<GetCurrentUserQuery, object> _getCurrentUserHandler;
        private readonly IQueryHandler<GetUserProfileQuery, object> _getUserProfileHandler;
        private readonly IQueryHandler<GetFollowersQuery, object> _getFollowersHandler;
        private readonly IQueryHandler<GetFollowingsQuery, object> _getFollowingsHandler;
        private readonly ICommandHandler<FollowUserCommand> _followUserHandler;
        private readonly ICommandHandler<UnfollowUserCommand> _unfollowUserHandler;
        private readonly ICommandHandler<UploadAvatarCommand, object> _uploadAvatarHandler;

        public UsersController(
            IQueryHandler<GetCurrentUserQuery, object> getCurrentUserHandler,
            IQueryHandler<GetUserProfileQuery, object> getUserProfileHandler,
            IQueryHandler<GetFollowersQuery, object> getFollowersHandler,
            IQueryHandler<GetFollowingsQuery, object> getFollowingsHandler,
            ICommandHandler<FollowUserCommand> followUserHandler,
            ICommandHandler<UnfollowUserCommand> unfollowUserHandler,
            ICommandHandler<UploadAvatarCommand, object> uploadAvatarHandler)
        {
            _getCurrentUserHandler = getCurrentUserHandler;
            _getUserProfileHandler = getUserProfileHandler;
            _getFollowersHandler = getFollowersHandler;
            _getFollowingsHandler = getFollowingsHandler;
            _followUserHandler = followUserHandler;
            _unfollowUserHandler = unfollowUserHandler;
            _uploadAvatarHandler = uploadAvatarHandler;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var query = new GetCurrentUserQuery { UserId = userId };
            var result = await _getCurrentUserHandler.HandleAsync(query);

            if (result == null)
                return NotFound();

            return Ok(result);
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
            var query = new GetUserProfileQuery { Username = username };
            var result = await _getUserProfileHandler.HandleAsync(query);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// Follow a user by their ID. Requires authentication.
        /// </summary>
        /// <param name="id">Target user ID to follow.</param>
        /// <returns>200 OK on success, 400 if invalid or already following.</returns>
        [HttpPost("{id}/follow")]
        public async Task<IActionResult> FollowUser(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var currentUserId))
                return Unauthorized();

            try
            {
                var command = new FollowUserCommand 
                { 
                    CurrentUserId = currentUserId, 
                    TargetUserId = id 
                };
                await _followUserHandler.HandleAsync(command);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Unfollow a user by their ID. Requires authentication.
        /// </summary>
        /// <param name="id">Target user ID to unfollow.</param>
        /// <returns>200 OK on success, 404 if no existing follow found.</returns>
        [HttpDelete("{id}/unfollow")]
        public async Task<IActionResult> UnfollowUser(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var currentUserId))
                return Unauthorized();

            try
            {
                var command = new UnfollowUserCommand 
                { 
                    CurrentUserId = currentUserId, 
                    TargetUserId = id 
                };
                await _unfollowUserHandler.HandleAsync(command);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
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
            var query = new GetFollowersQuery { UserId = id };
            var result = await _getFollowersHandler.HandleAsync(query);

            if (result == null)
                return NotFound(new { error = "User not found." });

            return Ok(result);
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
            var query = new GetFollowingsQuery { UserId = id };
            var result = await _getFollowingsHandler.HandleAsync(query);

            if (result == null)
                return NotFound(new { error = "User not found." });

            return Ok(result);
        }

        /// <summary>
        /// Upload or update your profile avatar.
        /// </summary>
        /// <param name="dto">Multipart/form-data containing image file.</param>
        /// <returns>Returns uploaded image URL</returns>
        [HttpPost("me/avatar")]
        public async Task<IActionResult> UploadAvatar([FromForm] AvatarUploadDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            try
            {
                var command = new UploadAvatarCommand { UserId = userId, Dto = dto };
                var result = await _uploadAvatarHandler.HandleAsync(command);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}