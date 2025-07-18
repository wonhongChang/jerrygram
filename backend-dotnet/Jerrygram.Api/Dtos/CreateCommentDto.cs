using System.ComponentModel.DataAnnotations;

namespace Jerrygram.Api.Dtos
{
    public class CreateCommentDto
    {
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;
    }
}
