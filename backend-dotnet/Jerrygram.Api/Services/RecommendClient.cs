using Jerrygram.Api.Dtos;
using System.Text.Json;

namespace Jerrygram.Api.Services
{
    public class RecommendClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RecommendClient> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public RecommendClient(HttpClient httpClient, ILogger<RecommendClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<PostListItemDto>> GetRecommendationsAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/recommend?userId={userId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Recommend service returned status {StatusCode}", response.StatusCode);
                    return [];
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<PostListItemDto>>(json, JsonOptions) ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling recommend service");
                return [];
            }
        }
    }
}
