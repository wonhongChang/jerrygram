using System.Text.Json;
using Application.Events;
using Application.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    public class SearchTrendStreamProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SearchTrendStreamProcessor> _logger;
        private readonly SearchTrendStreamProcessorSettings _settings;
        private readonly JsonSerializerOptions _jsonOptions;

        public SearchTrendStreamProcessor(
            IServiceScopeFactory scopeFactory,
            IOptions<SearchTrendStreamProcessorSettings> settings,
            ILogger<SearchTrendStreamProcessor> logger)
        {
            _scopeFactory = scopeFactory;
            _settings = settings.Value;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_settings.Enabled)
            {
                _logger.LogInformation("Search trend stream processor is disabled");
                return;
            }

            await Task.Yield();

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                GroupId = _settings.ConsumerGroup,
                ClientId = _settings.ClientId,
                AutoOffsetReset = ParseAutoOffsetReset(_settings.AutoOffsetReset),
                EnableAutoCommit = false,
                AllowAutoCreateTopics = false
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerConfig)
                .SetValueDeserializer(Deserializers.Utf8)
                .SetKeyDeserializer(Deserializers.Utf8)
                .SetErrorHandler((_, error) => _logger.LogWarning("Kafka search trend consumer error: {Reason}", error.Reason))
                .Build();

            try
            {
                consumer.Subscribe(_settings.Topic);
                _logger.LogInformation(
                    "Search trend stream processor subscribed to Kafka topic {Topic} with group {GroupId}",
                    _settings.Topic,
                    _settings.ConsumerGroup);

                while (!stoppingToken.IsCancellationRequested)
                {
                    ConsumeResult<string, string>? result = null;

                    try
                    {
                        result = consumer.Consume(TimeSpan.FromMilliseconds(_settings.PollTimeoutMs));
                        if (result?.Message?.Value == null)
                        {
                            continue;
                        }

                        var searchEvent = JsonSerializer.Deserialize<SearchEvent>(result.Message.Value, _jsonOptions);
                        if (searchEvent == null || string.IsNullOrWhiteSpace(searchEvent.SearchTerm))
                        {
                            consumer.Commit(result);
                            continue;
                        }

                        using var scope = _scopeFactory.CreateScope();
                        var trendWriter = scope.ServiceProvider.GetRequiredService<ISearchTrendWriter>();
                        await trendWriter.RecordSearchAsync(searchEvent, stoppingToken);

                        consumer.Commit(result);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Skipping malformed search event from Kafka");
                        if (result != null)
                        {
                            consumer.Commit(result);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogWarning(ex, "Failed to consume search trend event");
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to process search trend event");
                    }
                }
            }
            finally
            {
                consumer.Close();
            }
        }

        private static AutoOffsetReset ParseAutoOffsetReset(string value)
        {
            return Enum.TryParse<AutoOffsetReset>(value, ignoreCase: true, out var parsed)
                ? parsed
                : AutoOffsetReset.Earliest;
        }
    }
}
