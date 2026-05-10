using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace WebApi.Requests
{
    public class PostUploadRequest
    {
        public string? Caption { get; set; }
        public IFormFile? Image { get; set; }
        public PostVisibility Visibility { get; set; } = PostVisibility.Public;
    }
}
