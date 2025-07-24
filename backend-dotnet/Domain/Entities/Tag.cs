using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Tag
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        // Navigation: one-to-many
        public ICollection<PostTag> PostTags { get; set; } = [];
    }
}