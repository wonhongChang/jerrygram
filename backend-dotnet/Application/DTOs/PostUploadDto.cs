using Application.Common;
using Domain.Enums;

namespace Application.DTOs
{
    public class PostUploadDto
    {
        public string? Caption { get; set; }
        public UploadFile? Image { get; set; }
        public PostVisibility Visibility { get; set; } = PostVisibility.Public;
    }
}
