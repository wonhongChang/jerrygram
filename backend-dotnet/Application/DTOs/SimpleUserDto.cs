namespace Application.DTOs
{
    public class SimpleUserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
    }
}
