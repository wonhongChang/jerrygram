using Application.Events;
using Application.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class EventService : IEventService, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<EventService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public EventService(IProducer<string, string> producer, ILogger<EventService> logger)
        {
            _producer = producer;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task PublishSearchEventAsync(SearchEvent searchEvent, CancellationToken cancellationToken = default)
        {
            await PublishEventAsync("search-events", searchEvent.SearchTerm, searchEvent, cancellationToken);
        }

        public async Task PublishUserEventAsync(UserEvent userEvent, CancellationToken cancellationToken = default)
        {
            await PublishEventAsync("user-events", userEvent.UserId ?? "unknown", userEvent, cancellationToken);
        }

        public async Task PublishPostEventAsync(PostEvent postEvent, CancellationToken cancellationToken = default)
        {
            await PublishEventAsync("post-events", postEvent.PostId, postEvent, cancellationToken);
        }

        public async Task PublishEventAsync<T>(string topic, string key, T eventData, CancellationToken cancellationToken = default)
            where T : BaseEvent
        {
            try
            {
                var json = JsonSerializer.Serialize(eventData, _jsonOptions);

                var message = new Message<string, string>
                {
                    Key = key,
                    Value = json,
                    Headers = new Headers
                    {
                        { "event-type", Encoding.UTF8.GetBytes(typeof(T).Name) },
                        { "content-type", Encoding.UTF8.GetBytes("application/json") },
                        { "timestamp", Encoding.UTF8.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()) },
                        { "source", Encoding.UTF8.GetBytes("jerrygram-dotnet-backend") }
                    }
                };

                var deliveryReport = await _producer.ProduceAsync(topic, message, cancellationToken);

                _logger.LogDebug("Event published to {Topic} with key {Key} at offset {Offset}",
                    topic, key, deliveryReport.Offset);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Failed to publish {EventType} event to topic {Topic} with key {Key}",
                    typeof(T).Name, topic, key);
                // Don't rethrow to prevent breaking main application flow
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while publishing {EventType} event", typeof(T).Name);
                // Don't rethrow to prevent breaking main application flow
            }
        }

        public void Dispose()
        {
            try
            {
                _producer?.Flush(TimeSpan.FromSeconds(10));
                _producer?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing Kafka producer");
            }
        }
    }
}
