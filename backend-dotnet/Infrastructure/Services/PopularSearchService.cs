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

                var searchRequest = new SearchRequest<object>("jerrygram-events-*")
                {
                    Query = new BoolQuery
                    {
                        Must =
                        [
                            new TermQuery
                            {
                                Field = "kafka_topic",
                                Value = "search-events"
                            },
                            new DateRangeQuery
                            {
                                Field = "@timestamp",
                                GreaterThanOrEqualTo = startTime,
                                LessThanOrEqualTo = endTime
                            }
                        ]
                    },
                    Aggregations = new AggregationDictionary
                    {
                        {
                            "popular_terms", new TermsAggregation("popular_terms")
                            {
                                Field = "searchTerm.keyword",
                                Size = limit,
                                Order = [TermsOrder.CountDescending]
                            }
                        }
                    },
                    Size = 0
                };

                var response = await _elasticsearchClient.SearchAsync<object>(searchRequest);
                var popularSearches = new List<PopularSearchDto>();

                if (response.IsValid && response.Aggregations.TryGetValue("popular_terms", out var termsAgg))
                {
                    if (termsAgg is TermsAggregate<object> terms)
                    {
                        int rank = 1;
                        foreach (var bucket in terms.Buckets)
                        {
                            popularSearches.Add(new PopularSearchDto
                            {
                                SearchTerm = bucket.Key.ToString() ?? "",
                                Count = bucket.DocCount ?? 0,
                                Rank = rank++,
                                LastSearched = endTime,
                                Category = "stable"
                            });
                        }
                    }
                }

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
            // Compare last 6 hours vs previous 6 hours
            var current = await GetPopularSearchesAsync(limit * 2, TimeSpan.FromHours(6));
            var previous = await GetPopularSearchesAsync(limit * 2, TimeSpan.FromHours(12));

            var trending = current.Where(c =>
                !previous.Any(p => p.SearchTerm == c.SearchTerm) ||
                c.Count > (previous.FirstOrDefault(p => p.SearchTerm == c.SearchTerm)?.Count ?? 0))
                .Take(limit)
                .ToList();

            trending.ForEach(t => t.Category = "rising");
            return trending;
        }
    }
}
