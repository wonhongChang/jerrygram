using Jerrygram.Api.Data;
using Jerrygram.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Jerrygram.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<NotificationResponseDto>>> GetNotifications(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
                return BadRequest("Invalid pagination parameters.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var currentUserId = Guid.Parse(userId);

            var query = _context.Notifications
                .Where(n => n.RecipientId == currentUserId)
                .Include(n => n.FromUser)
                .OrderByDescending(n => n.CreatedAt);

            var totalCount = await query.CountAsync();

            var notifications = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationResponseDto
                {
                    Id = n.Id,
                    Message = n.Message ?? string.Empty,
                    Type = n.Type,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    PostId = n.PostId,
                    FromUser = new SimpleUserDto
                    {
                        Id = n.FromUser.Id,
                        Username = n.FromUser.Username,
                        ProfileImageUrl = n.FromUser.ProfileImageUrl
                    }
                })
                .ToListAsync();

            return Ok(new PagedResult<NotificationResponseDto>
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = notifications
            });
        }
    }
}
