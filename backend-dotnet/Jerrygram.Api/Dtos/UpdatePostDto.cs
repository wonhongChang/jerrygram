namespace Jerrygram.Api.Dtos
{
    public class UpdatePostDto
    {
        public string? Caption { get; set; }
        public IFormFile? Image { get; set; }
    }
}
