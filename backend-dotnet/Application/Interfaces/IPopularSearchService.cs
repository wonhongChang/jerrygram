using Application.DTOs;

namespace Application.Interfaces
{
    public interface IPopularSearchService
    {
        Task<List<PopularSearchDto>> GetPopularSearchesAsync(int limit = 10, TimeSpan? timeWindow = null);
        Task<List<PopularSearchDto>> GetTrendingSearchesAsync(int limit = 5);
    }
}
