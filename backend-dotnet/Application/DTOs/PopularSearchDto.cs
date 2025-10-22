namespace Application.DTOs
{
    public class PopularSearchDto
    {
        public string SearchTerm { get; set; } = string.Empty;
        public long Count { get; set; }
        public int Rank { get; set; }
        public DateTime LastSearched { get; set; }
        public string Category { get; set; } = "stable"; // "rising", "stable", "declining"
    }
}
