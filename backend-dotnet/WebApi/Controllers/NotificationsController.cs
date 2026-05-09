using Application.DTOs;
using Application.Interfaces;
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
        private readonly INotificationRepository _notificationRepository;

        public NotificationsController(
            IQueryHandler<GetNotificationsQuery, PagedResult<NotificationResponseDto>> getNotificationsHandler,
            INotificationRepository notificationRepository)
        {
            _getNotificationsHandler = getNotificationsHandler;
            _notificationRepository = notificationRepository;
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

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null || notification.RecipientId != userId)
                return NotFound();

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                _notificationRepository.Update(notification);
                await _notificationRepository.SaveChangesAsync();
            }

            return NoContent();
        }
    }
}
