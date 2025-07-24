using Application.DTOs;
using Application.Interfaces;

namespace Application.Queries.Notifications
{
    public class GetNotificationsQueryHandler : IQueryHandler<GetNotificationsQuery, PagedResult<NotificationResponseDto>>
    {
        private readonly INotificationRepository _notificationRepository;

        public GetNotificationsQueryHandler(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<PagedResult<NotificationResponseDto>> HandleAsync(GetNotificationsQuery query)
        {
            return await _notificationRepository.GetNotificationsAsync(query.UserId, query.Page, query.PageSize);
        }
    }
}