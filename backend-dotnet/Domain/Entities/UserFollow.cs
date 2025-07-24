using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class UserFollow
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid FollowerId { get; set; }

        [ForeignKey("FollowerId")]
        public User Follower { get; set; } = null!;

        [Required]
        public Guid FollowingId { get; set; }

        [ForeignKey("FollowingId")]
        public User Following { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}