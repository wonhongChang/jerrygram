using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<Notification?> GetLikeNotificationAsync(Guid postId, Guid fromUserId, Guid recipientId);
        Task<Notification?> GetFollowNotificationAsync(Guid fromUserId, Guid recipientId);
        Task<Notification?> GetCommentNotificationAsync(Guid postId, Guid fromUserId);
        Task<PagedResult<NotificationResponseDto>> GetNotificationsAsync(Guid userId, int page, int pageSize);
    }
}