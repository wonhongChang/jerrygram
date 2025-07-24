using Microsoft.AspNetCore.Http;
using Domain.Enums;

namespace Application.DTOs
{
    public class UpdatePostDto
    {
        public string? Caption { get; set; }
        public IFormFile? Image { get; set; }
        public PostVisibility? Visibility { get; set; }
    }
}
