using System.Threading.Channels;
using Application.Events;
using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public enum QueuedEventKind
    {
        Search,
        User,
        Post
    }

    public sealed record QueuedEvent(QueuedEventKind Kind, BaseEvent Event);

    public class QueuedEventPublisher : IEventPublisher
    {
        private readonly Channel<QueuedEvent> _channel;
        private readonly ILogger<QueuedEventPublisher> _logger;

        public QueuedEventPublisher(ILogger<QueuedEventPublisher> logger)
        {
            _logger = logger;
            _channel = Channel.CreateBounded<QueuedEvent>(new BoundedChannelOptions(1_000)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            });
        }

        public ValueTask QueueSearchEventAsync(SearchEvent searchEvent, CancellationToken cancellationToken = default)
        {
            return QueueAsync(new QueuedEvent(QueuedEventKind.Search, searchEvent), cancellationToken);
        }

        public ValueTask QueueUserEventAsync(UserEvent userEvent, CancellationToken cancellationToken = default)
        {
            return QueueAsync(new QueuedEvent(QueuedEventKind.User, userEvent), cancellationToken);
        }

        public ValueTask QueuePostEventAsync(PostEvent postEvent, CancellationToken cancellationToken = default)
        {
            return QueueAsync(new QueuedEvent(QueuedEventKind.Post, postEvent), cancellationToken);
        }

        internal IAsyncEnumerable<QueuedEvent> ReadAllAsync(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }

        private ValueTask QueueAsync(QueuedEvent queuedEvent, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return ValueTask.FromCanceled(cancellationToken);
            }

            if (!_channel.Writer.TryWrite(queuedEvent))
            {
                _logger.LogWarning("Failed to enqueue {EventKind} event", queuedEvent.Kind);
            }

            return ValueTask.CompletedTask;
        }
    }

    public class KafkaEventDispatchService : BackgroundService
    {
        private readonly QueuedEventPublisher _publisher;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<KafkaEventDispatchService> _logger;

        public KafkaEventDispatchService(
            QueuedEventPublisher publisher,
            IServiceScopeFactory scopeFactory,
            ILogger<KafkaEventDispatchService> logger)
        {
            _publisher = publisher;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var queuedEvent in _publisher.ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

                    switch (queuedEvent)
                    {
                        case { Kind: QueuedEventKind.Search, Event: SearchEvent searchEvent }:
                            await eventService.PublishSearchEventAsync(searchEvent, stoppingToken);
                            break;

                        case { Kind: QueuedEventKind.User, Event: UserEvent userEvent }:
                            await eventService.PublishUserEventAsync(userEvent, stoppingToken);
                            break;

                        case { Kind: QueuedEventKind.Post, Event: PostEvent postEvent }:
                            await eventService.PublishPostEventAsync(postEvent, stoppingToken);
                            break;

                        default:
                            _logger.LogWarning("Unsupported queued event kind {EventKind}", queuedEvent.Kind);
                            break;
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to dispatch queued {EventKind} event", queuedEvent.Kind);
                }
            }
        }
    }
}
