namespace Infrastructure.Services
{
    public class SearchTrendStreamProcessorSettings
    {
        public const string SectionName = "SearchTrendStreamProcessor";

        public bool Enabled { get; set; } = true;
        public string BootstrapServers { get; set; } = "localhost:19092";
        public string Topic { get; set; } = "search-events";
        public string ConsumerGroup { get; set; } = "jerrygram-search-trend-processor";
        public string ClientId { get; set; } = "jerrygram-search-trend-processor";
        public string AutoOffsetReset { get; set; } = "Earliest";
        public int PollTimeoutMs { get; set; } = 1000;
        public int RetentionHours { get; set; } = 168;
        public int BucketMinutes { get; set; } = 60;
    }
}
