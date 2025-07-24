using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entities
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RecipientId { get; set; }

        [ForeignKey("RecipientId")]
        public User Recipient { get; set; } = null!;

        [Required]
        public Guid FromUserId { get; set; }

        [ForeignKey("FromUserId")]
        public User FromUser { get; set; } = null!;

        [Required]
        public NotificationType Type { get; set; } // ex. Comment, Like, Follow

        public Guid? PostId { get; set; }

        [ForeignKey("PostId")]
        public Post? Post { get; set; }

        public string? Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}