using Application.Events;

namespace Application.Interfaces
{
    public interface IEventPublisher
    {
        ValueTask QueueSearchEventAsync(SearchEvent searchEvent, CancellationToken cancellationToken = default);
        ValueTask QueueUserEventAsync(UserEvent userEvent, CancellationToken cancellationToken = default);
        ValueTask QueuePostEventAsync(PostEvent postEvent, CancellationToken cancellationToken = default);
    }
}
