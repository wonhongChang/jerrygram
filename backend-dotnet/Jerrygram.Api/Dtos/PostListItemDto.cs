namespace Jerrygram.Api.Dtos
{
    public class PostListItemDto
    {
        public Guid Id { get; set; }
        public string? Caption { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Likes { get; set; }
        public bool Liked { get; set; }
        public SimpleUserDto User { get; set; } = null!;
    }
}
