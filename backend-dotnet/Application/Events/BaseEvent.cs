using System.Text.Json.Serialization;

namespace Application.Events
{
    public abstract class BaseEvent
    {
        [JsonPropertyName("eventId")]
        public string EventId { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        [JsonPropertyName("sessionId")]
        public string? SessionId { get; set; }

        [JsonPropertyName("ipAddress")]
        public string? IpAddress { get; set; }

        [JsonPropertyName("userAgent")]
        public string? UserAgent { get; set; }

        [JsonPropertyName("correlationId")]
        public string? CorrelationId { get; set; }
    }
}
