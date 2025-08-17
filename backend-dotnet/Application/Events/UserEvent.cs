using System.Text.Json.Serialization;

namespace Application.Events
{
    public class UserEvent : BaseEvent
    {
        [JsonPropertyName("eventType")]
        public string EventType { get; set; } = string.Empty; // "login", "register", "profile_view", "follow", "unfollow"

        [JsonPropertyName("targetUserId")]
        public string? TargetUserId { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
