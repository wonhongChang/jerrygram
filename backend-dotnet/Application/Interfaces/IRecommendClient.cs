using Application.DTOs;

namespace Application.Interfaces
{
    public interface IRecommendClient
    {
        Task<List<PostListItemDto>> GetRecommendationsAsync(Guid userId);
    }
}