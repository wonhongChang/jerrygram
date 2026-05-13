using Application.Events;

namespace Application.Interfaces
{
    public interface ISearchTrendWriter
    {
        Task RecordSearchAsync(SearchEvent searchEvent, CancellationToken cancellationToken = default);
    }
}
