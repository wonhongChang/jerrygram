using Jerrygram.Api.Models;
using Jerrygram.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Jerrygram.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;

        public AuthController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        // <summary>
        /// Login endpoint that generates a JWT token for a user.
        /// Note: This is a simplified version for demonstration only.
        /// In production, you should validate the user from the database.
        /// </summary>
        /// <param name="user">User object (mocked)</param>
        /// <returns>JWT token</returns>
        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {
            // ⚠️ In real case, validate the user credentials from the database here.

            var token = _jwtService.GenerateToken(user);
            return Ok(new { token });
        }

        /// <summary>
        /// Sample protected endpoint that requires JWT authentication.
        /// Returns the authenticated user's ID.
        /// </summary>
        /// <returns>User ID string</returns>
        [Authorize]
        [HttpGet("test")]
        public IActionResult GetMyInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Ok($"Authenticated user ID: {userId}");
        }
    }
}
