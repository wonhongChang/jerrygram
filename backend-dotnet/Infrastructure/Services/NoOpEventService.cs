using Application.Events;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class NoOpEventService : IEventService
    {
        private readonly ILogger<NoOpEventService> _logger;

        public NoOpEventService(ILogger<NoOpEventService> logger)
        {
            _logger = logger;
        }

        public Task PublishSearchEventAsync(SearchEvent searchEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Event publishing disabled - would publish search event: {SearchTerm}", searchEvent.SearchTerm);
            return Task.CompletedTask;
        }

        public Task PublishUserEventAsync(UserEvent userEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Event publishing disabled - would publish user event: {EventType}", userEvent.EventType);
            return Task.CompletedTask;
        }

        public Task PublishPostEventAsync(PostEvent postEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Event publishing disabled - would publish post event: {EventType} for post {PostId}",
                postEvent.EventType, postEvent.PostId);
            return Task.CompletedTask;
        }

        public Task PublishEventAsync<T>(string topic, string key, T eventData, CancellationToken cancellationToken = default) where T : BaseEvent
        {
            _logger.LogDebug("Event publishing disabled - would publish {EventType} to topic {Topic}",
                typeof(T).Name, topic);
            return Task.CompletedTask;
        }
    }
}
