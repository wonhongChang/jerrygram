using Application.DTOs;
using Application.Queries;
using Application.Queries.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly IQueryHandler<GetNotificationsQuery, PagedResult<NotificationResponseDto>> _getNotificationsHandler;

        public NotificationsController(IQueryHandler<GetNotificationsQuery, PagedResult<NotificationResponseDto>> getNotificationsHandler)
        {
            _getNotificationsHandler = getNotificationsHandler;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<NotificationResponseDto>>> GetNotifications(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
                return BadRequest("Invalid pagination parameters.");

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var query = new GetNotificationsQuery
            {
                UserId = userId,
                Page = page,
                PageSize = pageSize
            };

            var result = await _getNotificationsHandler.HandleAsync(query);
            return Ok(result);
        }
    }
}