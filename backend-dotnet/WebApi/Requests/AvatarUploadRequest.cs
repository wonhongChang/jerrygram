using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebApi.Requests
{
    public class AvatarUploadRequest
    {
        [Required]
        public IFormFile Avatar { get; set; } = null!;
    }
}
