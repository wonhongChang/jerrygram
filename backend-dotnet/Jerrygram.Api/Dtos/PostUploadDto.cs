using System.ComponentModel.DataAnnotations;

namespace Jerrygram.Api.Dtos
{
    public class PostUploadDto
    {
        public string? Caption { get; set; }
        public IFormFile? Image { get; set; }
    }
}
