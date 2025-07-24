using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class PostTag
    {
        [Required]
        public Guid PostId { get; set; }
        public Post Post { get; set; } = default!;
        [Required]
        public Guid TagId { get; set; }
        public Tag Tag { get; set; } = default!;
    }
}