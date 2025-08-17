namespace WebApi.Configurations
{
    public class KafkaSettings
    {
        public const string SectionName = "Kafka";

        public string BootstrapServers { get; set; } = "kafka:29092";
        public string SchemaRegistryUrl { get; set; } = "http://kafka-connect:8083";
        public bool EnableIdempotence { get; set; } = true;
        public string SecurityProtocol { get; set; } = "PLAINTEXT";
        public int MessageTimeoutMs { get; set; } = 30000;
        public int RequestTimeoutMs { get; set; } = 30000;
        public string ClientId { get; set; } = "jerrygram-dotnet-backend";
        public bool EnableEventPublishing { get; set; } = true;
    }
}
