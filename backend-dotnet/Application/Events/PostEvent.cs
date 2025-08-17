using System.Text.Json.Serialization;

namespace Application.Events
{
    public class PostEvent : BaseEvent
    {
        [JsonPropertyName("postId")]
        public string PostId { get; set; } = string.Empty;

        [JsonPropertyName("eventType")]
        public string EventType { get; set; } = string.Empty; // "create", "like", "unlike", "comment", "view", "share"

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
