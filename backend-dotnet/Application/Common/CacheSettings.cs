namespace Application.Common
{
    public class RedisCacheSettings
    {
        public string? Password { get; set; }
        public int Database { get; set; } = 0;
        public string KeyPrefix { get; set; } = "jg:";
        public int DefaultExpirationMinutes { get; set; } = 30;
        public int SlidingExpirationMinutes { get; set; } = 5;
        public int MaxRetries { get; set; } = 3;
        public int RetryDelayMs { get; set; } = 1000;
    }

    public class CacheSettings
    {
        public bool UseRedis { get; set; } = false;
        public int DefaultExpirationMinutes { get; set; } = 30;
        public int SlidingExpirationMinutes { get; set; } = 5;
        public bool EnableCaching { get; set; } = true;
    }
}
