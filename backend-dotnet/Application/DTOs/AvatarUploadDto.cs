using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class AvatarUploadDto
    {
        [Required]
        public IFormFile Avatar { get; set; } = null!;
    }
}
