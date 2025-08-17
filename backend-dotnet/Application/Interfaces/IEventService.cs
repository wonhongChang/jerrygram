using Application.Events;

namespace Application.Interfaces
{
    public interface IEventService
    {
        Task PublishSearchEventAsync(SearchEvent searchEvent, CancellationToken cancellationToken = default);
        Task PublishUserEventAsync(UserEvent userEvent, CancellationToken cancellationToken = default);
        Task PublishPostEventAsync(PostEvent postEvent, CancellationToken cancellationToken = default);
        Task PublishEventAsync<T>(string topic, string key, T eventData, CancellationToken cancellationToken = default) where T : BaseEvent;
    }
}
