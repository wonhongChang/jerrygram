using Application.DTOs;
using Application.Interfaces;
using Microsoft.Extensions.Logging;
using Nest;

namespace Infrastructure.Services
{
    public class PopularSearchService : IPopularSearchService
    {
        private readonly IElasticClient _elasticsearchClient;
        private readonly ICacheService _cacheService;
        private readonly ILogger<PopularSearchService> _logger;

        public PopularSearchService(
            IElasticClient elasticsearchClient,
            ICacheService cacheService,
            ILogger<PopularSearchService> logger)
        {
            _elasticsearchClient = elasticsearchClient;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<List<PopularSearchDto>> GetPopularSearchesAsync(int limit = 10, TimeSpan? timeWindow = null)
        {
            var window = timeWindow ?? TimeSpan.FromHours(24);
            var cacheKey = $"popular_searches_{limit}_{window.TotalMinutes}";

            var cached = await _cacheService.GetAsync<List<PopularSearchDto>>(cacheKey);
            if (cached != null) return cached;

            try
            {
                var endTime = DateTime.UtcNow;
                var startTime = endTime.Subtract(window);
                var popularSearches = await GetPopularSearchesBetweenAsync(startTime, endTime, limit, "stable");

                await _cacheService.SetAsync(cacheKey, popularSearches, TimeSpan.FromMinutes(5));
                return popularSearches;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving popular searches");
                return new List<PopularSearchDto>();
            }
        }
        public async Task<List<PopularSearchDto>> GetTrendingSearchesAsync(int limit = 5)
        {
            var endTime = DateTime.UtcNow;
            var currentStart = endTime.Subtract(TimeSpan.FromHours(6));
            var previousStart = endTime.Subtract(TimeSpan.FromHours(12));

            var current = await GetPopularSearchesBetweenAsync(currentStart, endTime, limit * 2, "rising");
            var previous = await GetPopularSearchesBetweenAsync(previousStart, currentStart, limit * 2, "previous");
            var previousByTerm = previous.ToDictionary(p => p.SearchTerm, StringComparer.OrdinalIgnoreCase);

            var trending = current.Where(c =>
                !previousByTerm.TryGetValue(c.SearchTerm, out var prior) ||
                c.Count > prior.Count)
                .Take(limit)
                .ToList();

            trending.ForEach(t => t.Category = "rising");
            return trending;
        }

        private async Task<List<PopularSearchDto>> GetPopularSearchesBetweenAsync(
            DateTime startTime,
            DateTime endTime,
            int limit,
            string category)
        {
            var response = await _elasticsearchClient.SearchAsync<object>(s => s
                .Index("jerrygram-events-*")
                .Size(0)
                .Query(q => q.Bool(b => b.Must(
                    m => m.Term("kafka_topic.keyword", "search-events"),
                    m => m.DateRange(r => r
                        .Field("@timestamp")
                        .GreaterThanOrEquals(startTime)
                        .LessThanOrEquals(endTime)))))
                .Aggregations(a => a.Terms("popular_terms", t => t
                    .Field("searchTerm.keyword")
                    .Size(limit))));

            if (!response.IsValid)
            {
                _logger.LogWarning("Popular search aggregation failed: {Reason}", response.ServerError?.Error?.Reason ?? response.OriginalException?.Message);
                return [];
            }

            var terms = response.Aggregations.Terms("popular_terms");
            if (terms?.Buckets == null)
                return [];

            var rank = 1;
            return terms.Buckets
                .Select(bucket => new PopularSearchDto
                {
                    SearchTerm = bucket.Key,
                    Count = bucket.DocCount ?? 0,
                    Rank = rank++,
                    LastSearched = endTime,
                    Category = category
                })
                .Where(dto => !string.IsNullOrWhiteSpace(dto.SearchTerm))
                .ToList();
        }
    }
}
