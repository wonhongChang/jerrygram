using Application.DTOs;
using Application.Events;
using Application.Interfaces;
using Application.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.Services
{
    public class RedisSearchTrendReadModel : ISearchTrendReadModel, ISearchTrendWriter
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisSearchTrendReadModel> _logger;
        private readonly SearchTrendStreamProcessorSettings _settings;
        private readonly RedisCacheSettings _redisSettings;

        public RedisSearchTrendReadModel(
            IConnectionMultiplexer redis,
            IOptions<SearchTrendStreamProcessorSettings> settings,
            IOptions<RedisCacheSettings> redisSettings,
            ILogger<RedisSearchTrendReadModel> logger)
        {
            _database = redis.GetDatabase(redisSettings.Value.Database);
            _settings = settings.Value;
            _redisSettings = redisSettings.Value;
            _logger = logger;
        }

        public async Task RecordSearchAsync(SearchEvent searchEvent, CancellationToken cancellationToken = default)
        {
            var term = NormalizeSearchTerm(searchEvent.SearchTerm);
            if (string.IsNullOrWhiteSpace(term))
            {
                return;
            }

            try
            {
                var bucket = FloorToBucket(searchEvent.Timestamp == default ? DateTime.UtcNow : searchEvent.Timestamp);
                var key = GetBucketKey(bucket);

                await _database.SortedSetIncrementAsync(key, term, 1);
                await _database.KeyExpireAsync(key, TimeSpan.FromHours(_settings.RetentionHours + 2));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update Redis search trend bucket for term {SearchTerm}", term);
                throw;
            }
        }

        public async Task<List<PopularSearchDto>> GetPopularSearchesAsync(int limit, TimeSpan timeWindow)
        {
            if (limit <= 0)
            {
                return [];
            }

            var endTime = DateTime.UtcNow;
            var startTime = endTime.Subtract(timeWindow);
            return await GetPopularSearchesBetweenAsync(startTime, endTime, limit, "stable");
        }

        public async Task<List<PopularSearchDto>> GetTrendingSearchesAsync(int limit)
        {
            if (limit <= 0)
            {
                return [];
            }

            try
            {
                var endTime = DateTime.UtcNow;
                var currentStart = endTime.Subtract(TimeSpan.FromHours(6));
                var previousStart = endTime.Subtract(TimeSpan.FromHours(12));

                var current = await GetCountsBetweenAsync(currentStart, endTime);
                if (current.Count == 0)
                {
                    return [];
                }

                var previous = await GetCountsBetweenAsync(previousStart, currentStart);
                var rank = 1;

                return current
                    .Select(pair =>
                    {
                        previous.TryGetValue(pair.Key, out var previousCount);
                        return new
                        {
                            SearchTerm = pair.Key,
                            Count = pair.Value,
                            Delta = pair.Value - previousCount
                        };
                    })
                    .Where(item => item.Delta > 0)
                    .OrderByDescending(item => item.Delta)
                    .ThenByDescending(item => item.Count)
                    .ThenBy(item => item.SearchTerm, StringComparer.OrdinalIgnoreCase)
                    .Take(limit)
                    .Select(item => new PopularSearchDto
                    {
                        SearchTerm = item.SearchTerm,
                        Count = item.Count,
                        Rank = rank++,
                        LastSearched = endTime,
                        Category = "rising"
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read Redis trending search buckets");
                return [];
            }
        }

        private async Task<List<PopularSearchDto>> GetPopularSearchesBetweenAsync(
            DateTime startTime,
            DateTime endTime,
            int limit,
            string category)
        {
            try
            {
                var counts = await GetCountsBetweenAsync(startTime, endTime);
                var rank = 1;

                return counts
                    .OrderByDescending(pair => pair.Value)
                    .ThenBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                    .Take(limit)
                    .Select(pair => new PopularSearchDto
                    {
                        SearchTerm = pair.Key,
                        Count = pair.Value,
                        Rank = rank++,
                        LastSearched = endTime,
                        Category = category
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read Redis search trend buckets");
                return [];
            }
        }

        private async Task<Dictionary<string, long>> GetCountsBetweenAsync(DateTime startTime, DateTime endTime)
        {
            var buckets = GetBucketStarts(startTime, endTime);
            if (buckets.Count == 0)
            {
                return [];
            }

            var counts = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            var bucketEntries = await Task.WhenAll(buckets.Select(bucket =>
                _database.SortedSetRangeByRankWithScoresAsync(GetBucketKey(bucket), 0, -1, Order.Descending)));

            foreach (var entries in bucketEntries)
            {
                foreach (var entry in entries)
                {
                    var term = entry.Element.ToString();
                    if (string.IsNullOrWhiteSpace(term))
                    {
                        continue;
                    }

                    counts.TryGetValue(term, out var currentCount);
                    counts[term] = currentCount + Convert.ToInt64(entry.Score);
                }
            }

            return counts;
        }

        private List<DateTime> GetBucketStarts(DateTime startTime, DateTime endTime)
        {
            var starts = new List<DateTime>();
            var current = FloorToBucket(startTime);
            var cappedEndTime = endTime > DateTime.UtcNow ? DateTime.UtcNow : endTime;

            while (current <= cappedEndTime)
            {
                starts.Add(current);
                current = current.AddMinutes(GetBucketMinutes());
            }

            return starts;
        }

        private DateTime FloorToBucket(DateTime timestamp)
        {
            var utc = timestamp.Kind == DateTimeKind.Utc ? timestamp : timestamp.ToUniversalTime();
            var bucketTicks = TimeSpan.FromMinutes(GetBucketMinutes()).Ticks;
            return new DateTime(utc.Ticks - utc.Ticks % bucketTicks, DateTimeKind.Utc);
        }

        private int GetBucketMinutes()
        {
            return Math.Max(1, _settings.BucketMinutes);
        }

        private string GetBucketKey(DateTime bucketStart)
        {
            return $"{_redisSettings.KeyPrefix}search-trends:{bucketStart:yyyyMMddHHmm}";
        }

        private static string NormalizeSearchTerm(string? searchTerm)
        {
            return searchTerm?.Trim().ToLowerInvariant() ?? string.Empty;
        }
    }
}
