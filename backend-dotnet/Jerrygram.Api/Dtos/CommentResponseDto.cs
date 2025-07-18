namespace Jerrygram.Api.Dtos
{
    public class CommentResponseDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public SimpleUserDto User { get; set; } = null!;
    }
}
