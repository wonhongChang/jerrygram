using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CacheService> _logger;
        private static readonly ConcurrentDictionary<string, byte> CacheKeys = new();

        public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                if (_cache.TryGetValue(key, out var cached))
                {
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                    
                    if (cached is T directValue)
                        return Task.FromResult<T?>(directValue);
                    
                    if (cached is string jsonValue)
                        return Task.FromResult(JsonSerializer.Deserialize<T>(jsonValue));
                }

                _logger.LogDebug("Cache miss for key: {Key}", key);
                return Task.FromResult<T?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache for key: {Key}", key);
                return Task.FromResult<T?>(null);
            }
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
        {
            try
            {
                var options = new MemoryCacheEntryOptions();
                
                if (expiry.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiry.Value;
                }
                else
                {
                    // Default expiration: 30 minutes
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                }

                // Set sliding expiration to 5 minutes
                options.SlidingExpiration = TimeSpan.FromMinutes(5);
                
                // Set cache priority
                options.Priority = CacheItemPriority.Normal;

                _cache.Set(key, value, options);
                CacheKeys.TryAdd(key, 0);
                _logger.LogDebug("Cache set for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for key: {Key}", key);
            }

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            try
            {
                _cache.Remove(key);
                CacheKeys.TryRemove(key, out _);
                _logger.LogDebug("Cache removed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache for key: {Key}", key);
            }

            return Task.CompletedTask;
        }

        public void RemoveByPattern(string pattern)
        {
            try
            {
                var normalizedPattern = pattern.Replace("*", string.Empty);
                var keysToRemove = CacheKeys.Keys
                    .Where(key => key.Contains(normalizedPattern, StringComparison.Ordinal))
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                    CacheKeys.TryRemove(key, out _);
                }

                _logger.LogDebug("Removed {Count} cache entries matching pattern: {Pattern}",
                    keysToRemove.Count, pattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache by pattern: {Pattern}", pattern);
            }
        }
    }

    public static class CacheKeys
    {
        public const string UserFeed = "user_feed_{0}_page_{1}";
        public const string PublicPosts = "public_posts_page_{0}";
        public const string PostLikes = "post_likes_{0}_page_{1}";
        public const string UserProfile = "user_profile_{0}";
        public const string PostDetails = "post_details_{0}";
        public const string ExplorePosts = "explore_posts_{0}";
        public const string SearchResults = "search_{0}_page_{1}";
    }
}
