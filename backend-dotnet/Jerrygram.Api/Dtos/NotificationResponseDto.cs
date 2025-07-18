using Jerrygram.Api.Models;

namespace Jerrygram.Api.Dtos
{
    public class NotificationResponseDto
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public SimpleUserDto FromUser { get; set; } = null!;
        public Guid? PostId { get; set; }
    }
}
