using Application.DTOs;

namespace Application.Interfaces
{
    public interface ISearchTrendReadModel
    {
        Task<List<PopularSearchDto>> GetPopularSearchesAsync(int limit, TimeSpan timeWindow);
        Task<List<PopularSearchDto>> GetTrendingSearchesAsync(int limit);
    }
}
