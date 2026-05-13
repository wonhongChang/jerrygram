# Environment and Secret Setup

Language: English | [한국어](env-and-secrets.ko.md) | [日本語](env-and-secrets.ja.md)

Jerrygram keeps local secrets out of Git. Commit the `*.example` files, then copy them into local runtime files when setting up a machine.

## Local Files

| Purpose | Example file | Local file |
| --- | --- | --- |
| Docker port and shared secrets | `.env.example` | `.env` |
| .NET API settings | `backend-dotnet/WebApi/appsettings.example.json` | `backend-dotnet/WebApi/appsettings.json` |
| React dev server | `frontend-react/.env.example` | `frontend-react/.env` |
| Node recommendation service | `jerrygram-recommend/.env.example` | `jerrygram-recommend/.env` |
| Java backend | `backend-java/.env.example` | `backend-java/.env` |

## Current Local Port Map

| Service | Host port |
| --- | ---: |
| React web | `13000` |
| .NET API | `5018` |
| Recommendation service | `13001` |
| PostgreSQL | `15433` |
| Redis | `16380` |
| Elasticsearch | `19200` |
| Kafka broker | `19092` |
| Kafka UI | `18081` |
| Kafka Connect | `18083` |
| Kibana | `15601` |
| Logstash API | `19600` |

## Stream Processing Settings

`SearchTrendStreamProcessor` consumes Kafka `search-events` and writes Redis search trend buckets. Keep the consumer group stable locally so Kafka offsets are reused across restarts:

```json
"SearchTrendStreamProcessor": {
  "Enabled": true,
  "BootstrapServers": "localhost:19092",
  "ConsumerGroup": "jerrygram-search-trend-processor"
}
```

## GitHub Actions

The CI workflow builds and tests the project without production secrets. Runtime-only secrets such as `OPENAI_API_KEY`, Azure storage credentials, or deployment credentials should be added later as GitHub repository secrets only when a deploy workflow is introduced.

## Notes

- Keep `JwtSettings:SecretKey`, database passwords, Redis passwords, and cloud storage connection strings out of committed config.
- The Docker ports are intentionally shifted away from common defaults to reduce collisions with other local projects.
- Local blob storage uses the configured storage provider. `UseDevelopmentStorage=true` targets Azurite-compatible local storage; a real Azure connection string can be used without code changes.
