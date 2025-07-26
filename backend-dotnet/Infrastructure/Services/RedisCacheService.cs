using Application.Common;
using Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly RedisCacheSettings _settings;

        public RedisCacheService(
            IDistributedCache distributedCache,
            IConnectionMultiplexer redis,
            ILogger<RedisCacheService> logger,
            IOptions<RedisCacheSettings> settings)
        {
            _distributedCache = distributedCache;
            _redis = redis;
            _settings = settings.Value;
            _database = redis.GetDatabase(_settings.Database);
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var prefixedKey = GetPrefixedKey(key);
                var cached = await _distributedCache.GetStringAsync(prefixedKey);

                if (cached == null)
                {
                    _logger.LogDebug("Redis cache miss for key: {Key}", key);
                    return null;
                }

                _logger.LogDebug("Redis cache hit for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(cached);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving from Redis cache for key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
        {
            try
            {
                var prefixedKey = GetPrefixedKey(key);
                var serialized = JsonSerializer.Serialize(value);

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes),
                    SlidingExpiration = TimeSpan.FromMinutes(_settings.SlidingExpirationMinutes)
                };

                await _distributedCache.SetStringAsync(prefixedKey, serialized, options);
                _logger.LogDebug("Redis cache set for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Redis cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                var prefixedKey = GetPrefixedKey(key);
                await _distributedCache.RemoveAsync(prefixedKey);
                _logger.LogDebug("Redis cache removed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from Redis cache for key: {Key}", key);
            }
        }

        public void RemoveByPattern(string pattern)
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var prefixedPattern = GetPrefixedKey(pattern + "*");
                var keys = server.Keys(pattern: prefixedPattern).ToArray();

                if (keys.Any())
                {
                    _database.KeyDelete(keys);
                    _logger.LogDebug("Removed {Count} Redis cache entries matching pattern: {Pattern}",
                        keys.Length, pattern);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing Redis cache by pattern: {Pattern}", pattern);
            }
        }

        private string GetPrefixedKey(string key)
        {
            return $"{_settings.KeyPrefix}{key}";
        }
    }
}
