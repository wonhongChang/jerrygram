using Persistence.Data;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Notification?> GetLikeNotificationAsync(Guid postId, Guid fromUserId, Guid recipientId)
        {
            return await _dbSet.FirstOrDefaultAsync(n =>
                n.Type == NotificationType.Like &&
                n.PostId == postId &&
                n.FromUserId == fromUserId &&
                n.RecipientId == recipientId);
        }

        public async Task<Notification?> GetFollowNotificationAsync(Guid fromUserId, Guid recipientId)
        {
            return await _dbSet.FirstOrDefaultAsync(n =>
                n.Type == NotificationType.Follow &&
                n.FromUserId == fromUserId &&
                n.RecipientId == recipientId);
        }

        public async Task<Notification?> GetCommentNotificationAsync(Guid postId, Guid fromUserId)
        {
            return await _dbSet.FirstOrDefaultAsync(n =>
                n.Type == NotificationType.Comment &&
                n.PostId == postId &&
                n.FromUserId == fromUserId);
        }

        public async Task<PagedResult<NotificationResponseDto>> GetNotificationsAsync(Guid userId, int page, int pageSize)
        {
            var query = _dbSet
                .Where(n => n.RecipientId == userId)
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

            return new PagedResult<NotificationResponseDto>
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = notifications
            };
        }
    }
}