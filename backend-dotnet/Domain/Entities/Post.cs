using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.ValueObjects;
using Domain.Enums;

namespace Domain.Entities
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

        private string? _caption;

        [MaxLength(2200)]
        public string? Caption 
        { 
            get => _caption;
            set => _caption = string.IsNullOrEmpty(value) ? null : PostCaption.Create(value).Value;
        }

        [NotMapped]
        public PostCaption PostCaption => PostCaption.Create(_caption);

        [NotMapped]
        public IReadOnlyList<string> Hashtags => PostCaption.Hashtags;

        [NotMapped]
        public IReadOnlyList<string> Mentions => PostCaption.Mentions;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK
        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public ICollection<Comment> Comments { get; set; } = [];
        public ICollection<PostLike> Likes { get; set; } = [];
        public ICollection<PostTag> PostTags { get; set; } = [];

        /// <summary>
        /// Visibility of the post (Public, FollowersOnly, Private)
        /// </summary>
        [Required]
        public PostVisibility Visibility { get; set; } = PostVisibility.Public;
    }
}