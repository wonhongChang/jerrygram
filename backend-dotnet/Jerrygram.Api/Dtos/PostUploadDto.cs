using Jerrygram.Api.Models;

namespace Jerrygram.Api.Dtos
{
    public class PostUploadDto
    {
        public string? Caption { get; set; }
        public IFormFile? Image { get; set; }
        public PostVisibility Visibility { get; set; } = PostVisibility.Public;
    }
}
