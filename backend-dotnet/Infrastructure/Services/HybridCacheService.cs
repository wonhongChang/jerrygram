using Application.Common;
using Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    public class HybridCacheService : ICacheService
    {
        private readonly RedisCacheService _redisCache;
        private readonly CacheService _memoryCache;
        private readonly ILogger<HybridCacheService> _logger;
        private readonly CacheSettings _settings;

        public HybridCacheService(
            RedisCacheService redisCache,
            CacheService memoryCache,
            ILogger<HybridCacheService> logger,
            IOptions<CacheSettings> settings)
        {
            _redisCache = redisCache;
            _memoryCache = memoryCache;
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            if (!_settings.EnableCaching)
                return null;

            // Use Redis if enabled, otherwise fall back to memory cache
            if (_settings.UseRedis)
            {
                try
                {
                    var result = await _redisCache.GetAsync<T>(key);
                    if (result != null)
                        return result;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Redis cache failed for key: {Key}, falling back to memory cache", key);
                }
            }

            return await _memoryCache.GetAsync<T>(key);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
        {
            if (!_settings.EnableCaching)
                return;

            // Set in both caches for redundancy
            var tasks = new List<Task>();

            if (_settings.UseRedis)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await _redisCache.SetAsync(key, value, expiry);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to set Redis cache for key: {Key}", key);
                    }
                }));
            }

            tasks.Add(_memoryCache.SetAsync(key, value, expiry));
            await Task.WhenAll(tasks);
        }

        public async Task RemoveAsync(string key)
        {
            if (!_settings.EnableCaching)
                return;

            var tasks = new List<Task>();

            if (_settings.UseRedis)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await _redisCache.RemoveAsync(key);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to remove from Redis cache for key: {Key}", key);
                    }
                }));
            }

            tasks.Add(_memoryCache.RemoveAsync(key));
            await Task.WhenAll(tasks);
        }

        public void RemoveByPattern(string pattern)
        {
            if (!_settings.EnableCaching)
                return;

            if (_settings.UseRedis)
            {
                try
                {
                    _redisCache.RemoveByPattern(pattern);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to remove Redis cache by pattern: {Pattern}", pattern);
                }
            }

            _memoryCache.RemoveByPattern(pattern);
        }
    }
}
