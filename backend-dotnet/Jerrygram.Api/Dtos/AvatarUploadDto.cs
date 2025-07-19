using System.ComponentModel.DataAnnotations;

namespace Jerrygram.Api.Dtos
{
    public class AvatarUploadDto
    {
        [Required]
        public IFormFile Avatar { get; set; } = null!;
    }
}
