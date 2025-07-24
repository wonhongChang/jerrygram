using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CreateCommentDto
    {
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;
    }
}
