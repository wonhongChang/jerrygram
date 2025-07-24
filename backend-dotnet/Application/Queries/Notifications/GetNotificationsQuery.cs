using Application.DTOs;

namespace Application.Queries.Notifications
{
    public class GetNotificationsQuery : IQuery<PagedResult<NotificationResponseDto>>
    {
        public Guid UserId { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}