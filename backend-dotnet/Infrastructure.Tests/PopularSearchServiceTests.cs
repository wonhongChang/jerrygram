using System.Text;
using Application.DTOs;
using Application.Interfaces;
using Elasticsearch.Net;
using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Nest;
using Xunit;

namespace Infrastructure.Tests;

public class PopularSearchServiceTests
{
    [Fact]
    public async Task GetPopularSearchesAsync_FiltersSearchEventsWithKafkaTopicFields()
    {
        var requestBody = Array.Empty<byte>();
        var responseBody = Encoding.UTF8.GetBytes("""
        {
          "took": 1,
          "timed_out": false,
          "_shards": { "total": 1, "successful": 1, "skipped": 0, "failed": 0 },
          "hits": { "total": { "value": 0, "relation": "eq" }, "max_score": null, "hits": [] },
          "aggregations": {
            "popular_terms": {
              "doc_count_error_upper_bound": 0,
              "sum_other_doc_count": 0,
              "buckets": [
                { "key": "kafka", "doc_count": 3 }
              ]
            }
          }
        }
        """);

        var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
        var connection = new InMemoryConnection(responseBody, 200);
        var settings = new ConnectionSettings(pool, connection)
            .DisableDirectStreaming()
            .OnRequestCompleted(call =>
            {
                requestBody = call.RequestBodyInBytes ?? Array.Empty<byte>();
            });

        var service = new PopularSearchService(
            new ElasticClient(settings),
            new NullCacheService(),
            new NullSearchTrendReadModel(),
            NullLogger<PopularSearchService>.Instance);

        var results = await service.GetPopularSearchesAsync(10, TimeSpan.FromHours(24));

        var result = Assert.Single(results);
        Assert.Equal("kafka", result.SearchTerm);
        Assert.Equal(3, result.Count);

        var query = Encoding.UTF8.GetString(requestBody);
        Assert.Contains("\"kafka_topic\":{\"value\":\"search-events\"}", query);
        Assert.Contains("\"kafka_topic.keyword\":{\"value\":\"search-events\"}", query);
        Assert.Contains("\"event_category.keyword\":{\"value\":\"search\"}", query);
        Assert.Contains("\"field\":\"searchTerm.keyword\"", query);
    }

    [Fact]
    public async Task GetPopularSearchesAsync_PrefersStreamReadModelBeforeElasticsearchFallback()
    {
        var requestBody = Array.Empty<byte>();
        var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
        var connection = new InMemoryConnection(Encoding.UTF8.GetBytes("{}"), 200);
        var settings = new ConnectionSettings(pool, connection)
            .DisableDirectStreaming()
            .OnRequestCompleted(call =>
            {
                requestBody = call.RequestBodyInBytes ?? Array.Empty<byte>();
            });

        var streamResults = new List<PopularSearchDto>
        {
            new()
            {
                SearchTerm = "kafka",
                Count = 5,
                Rank = 1,
                LastSearched = DateTime.UtcNow,
                Category = "stable"
            }
        };

        var service = new PopularSearchService(
            new ElasticClient(settings),
            new NullCacheService(),
            new FakeSearchTrendReadModel(streamResults),
            NullLogger<PopularSearchService>.Instance);

        var results = await service.GetPopularSearchesAsync(10, TimeSpan.FromHours(24));

        var result = Assert.Single(results);
        Assert.Equal("kafka", result.SearchTerm);
        Assert.Equal(5, result.Count);
        Assert.Empty(requestBody);
    }

    private sealed class NullCacheService : ICacheService
    {
        public Task<T?> GetAsync<T>(string key) where T : class => Task.FromResult<T?>(null);

        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class => Task.CompletedTask;

        public Task RemoveAsync(string key) => Task.CompletedTask;

        public void RemoveByPattern(string pattern)
        {
        }
    }

    private sealed class NullSearchTrendReadModel : ISearchTrendReadModel
    {
        public Task<List<PopularSearchDto>> GetPopularSearchesAsync(int limit, TimeSpan timeWindow)
        {
            return Task.FromResult(new List<PopularSearchDto>());
        }

        public Task<List<PopularSearchDto>> GetTrendingSearchesAsync(int limit)
        {
            return Task.FromResult(new List<PopularSearchDto>());
        }
    }

    private sealed class FakeSearchTrendReadModel : ISearchTrendReadModel
    {
        private readonly List<PopularSearchDto> _popularSearches;

        public FakeSearchTrendReadModel(List<PopularSearchDto> popularSearches)
        {
            _popularSearches = popularSearches;
        }

        public Task<List<PopularSearchDto>> GetPopularSearchesAsync(int limit, TimeSpan timeWindow)
        {
            return Task.FromResult(_popularSearches.Take(limit).ToList());
        }

        public Task<List<PopularSearchDto>> GetTrendingSearchesAsync(int limit)
        {
            return Task.FromResult(new List<PopularSearchDto>());
        }
    }
}
