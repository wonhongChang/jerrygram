# Jerrygram

Language: [English](README.md) | [한국어](README.ko.md) | 日本語

Jerrygram は、React、ASP.NET Core、PostgreSQL、Redis、Blob Storage、Elasticsearch、Kafka、Logstash、Kibana、Node.js recommendation service を組み合わせた Instagram 風の social app です。

現在の標準確認ルートは次の通りです。

- React web UI: `http://localhost:13000`
- ASP.NET Core Web API: `http://localhost:5018`
- Docker infrastructure: PostgreSQL, Redis, Elasticsearch, Kafka, Kafka UI, Logstash, Kibana, Kafka Connect, recommendation service

この repository には Java/Spring Boot backend も含まれています。現在の web UI は default で .NET API に接続します。

## Architecture

![Jerrygram architecture](docs/assets/jerrygram-architecture.png)

Backend internals are documented in [docs/backend-architecture.ja.md](docs/backend-architecture.ja.md).

## Screenshots

![Register screen](docs/assets/screenshots/jerrygram-register.png)

![Feed screen](docs/assets/screenshots/jerrygram-feed.png)

![Search trends screen](docs/assets/screenshots/jerrygram-search.png)

## Docs

- [Backend architecture](docs/backend-architecture.ja.md)
- [Recommendation and Kafka evidence](docs/recommendation-and-kafka.ja.md)
- [Elasticsearch index inventory](docs/elasticsearch-indexes.ja.md)
- [Environment and secret setup](docs/env-and-secrets.ja.md)
- [Seed data](infra/seed/README.ja.md)

## Component READMEs

- [.NET backend](backend-dotnet/README.ja.md)
- [Java backend](backend-java/README.ja.md)
- [React frontend](frontend-react/README.ja.md)
- [Recommendation service](jerrygram-recommend/README.ja.md)

## Main Features

- JWT based register, login, logout, and current-user loading
- Multipart image upload for posts
- Home feed, public posts, post detail, explore, profile, and search screens
- Likes, comments, follows, notifications, profile editing, and saved posts
- Redis cache with in-memory fallback
- Elasticsearch based search and discovery
- Kafka event publishing from the .NET API
- Analytics pipeline from Kafka through Logstash/Kafka Connect to Elasticsearch
- Kibana support for `jerrygram-events-*`
- Node.js recommendation service using caption embeddings and cosine similarity
- Playwright E2E tests

## Local Ports

Jerrygram uses adjusted host ports to avoid collisions with other Docker projects.

| Service | URL / host port |
| --- | --- |
| React web UI | `http://localhost:13000` |
| ASP.NET Core API | `http://localhost:5018` |
| Recommendation service | `http://localhost:13001` |
| PostgreSQL | `localhost:15433` |
| Redis | `localhost:16380` |
| Elasticsearch | `http://localhost:19200` |
| Kafka | `localhost:19092` |
| Kafka UI | `http://localhost:18081` |
| Kibana | `http://localhost:15601` |
| Logstash API | `http://localhost:19600` |
| Kafka Connect | `http://localhost:18083` |

## Setup

```powershell
Copy-Item .env.example .env
Copy-Item backend-dotnet/WebApi/appsettings.example.json backend-dotnet/WebApi/appsettings.json
Copy-Item frontend-react/.env.example frontend-react/.env
Copy-Item jerrygram-recommend/.env.example jerrygram-recommend/.env
Copy-Item backend-java/.env.example backend-java/.env
```

For secrets, see [docs/env-and-secrets.ja.md](docs/env-and-secrets.ja.md).

## Run Docker Infrastructure

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka-elk-extended.yml up -d
```

## Run .NET API

```powershell
dotnet run --project backend-dotnet/WebApi/WebApi.csproj --urls http://localhost:5018
```

## Run React Web

```powershell
cd frontend-react
npm install
npm start
```

## Seed And Evidence

```powershell
powershell -ExecutionPolicy Bypass -File .\infra\seed\seed-jerrygram.ps1
powershell -ExecutionPolicy Bypass -File .\infra\seed\verify-jerrygram-demo.ps1
```

## Verification

```powershell
dotnet build backend-dotnet/WebApi/WebApi.csproj --configuration Release
dotnet test backend-dotnet/Domain.Tests/Domain.Tests.csproj --configuration Release
dotnet test backend-dotnet/Infrastructure.Tests/Infrastructure.Tests.csproj --configuration Release
```

```powershell
cd frontend-react
npm run test:ci
npm run e2e
```

```powershell
cd jerrygram-recommend
npm run lint
npm audit --omit=dev
```
