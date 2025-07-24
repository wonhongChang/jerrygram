using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs
{
    public class PostUploadDto
    {
        public string? Caption { get; set; }
        public IFormFile? Image { get; set; }
        public PostVisibility Visibility { get; set; } = PostVisibility.Public;
    }
}
