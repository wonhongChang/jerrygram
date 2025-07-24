using Domain.Enums;

namespace Application.Common
{
    public class PostIndex
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Caption { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public string Username { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public PostVisibility Visibility { get; set; }
    }
}