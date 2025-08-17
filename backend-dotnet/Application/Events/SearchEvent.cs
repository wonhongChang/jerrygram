using System.Text.Json.Serialization;

namespace Application.Events
{
    public class SearchEvent : BaseEvent
    {
        [JsonPropertyName("searchTerm")]
        public string SearchTerm { get; set; } = string.Empty;

        [JsonPropertyName("searchType")]
        public string SearchType { get; set; } = "general"; // "posts", "users", "hashtags", "general"

        [JsonPropertyName("resultCount")]
        public int ResultCount { get; set; }

        [JsonPropertyName("searchDuration")]
        public long SearchDurationMs { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
