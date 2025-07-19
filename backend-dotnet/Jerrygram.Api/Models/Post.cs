using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jerrygram.Api.Models
{
    public class Post
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Azure Blob URL
        /// </summary>
        [MaxLength(300)]
        public string? ImageUrl { get; set; } = string.Empty;

        [MaxLength(2200)]
        public string? Caption { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK
        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public ICollection<Comment> Comments { get; set; } = [];
        public ICollection<PostLike> Likes { get; set; } = [];

        /// <summary>
        /// Visibility of the post (Public, FollowersOnly, Private)
        /// </summary>
        [Required]
        public PostVisibility Visibility { get; set; } = PostVisibility.Public;
    }
}
