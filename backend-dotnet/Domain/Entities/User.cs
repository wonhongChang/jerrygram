using System.ComponentModel.DataAnnotations;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        private string _username = string.Empty;
        private string _email = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Username 
        { 
            get => _username;
            set => _username = ValueObjects.Username.Create(value).Value;
        }

        [Required]
        [MaxLength(100)]
        public string Email 
        { 
            get => _email;
            set => _email = ValueObjects.Email.Create(value).Value;
        }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? ProfileImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Post> Posts { get; set; } = [];
        public ICollection<Comment> Comments { get; set; } = [];
        public ICollection<UserFollow> Followers { get; set; } = [];
        public ICollection<UserFollow> Followings { get; set; } = [];
        public ICollection<PostLike> Likes { get; set; } = [];
        public ICollection<Notification> Notifications { get; set; } = [];
    }
}