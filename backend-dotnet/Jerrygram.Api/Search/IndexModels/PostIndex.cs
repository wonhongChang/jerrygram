using Jerrygram.Api.Models;

namespace Jerrygram.Api.Search.IndexModels
{
    public class PostIndex
    {
        public Guid Id { get; set; }
        public string? Caption { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Visibility { get; set; } = PostVisibility.Public.ToString();
    }
}
