# Jerrygram

Language: English | [한국어](README.ko.md) | [日本語](README.ja.md)

[![Build and test](https://github.com/wonhongChang/jerrygram/actions/workflows/ci.yml/badge.svg)](https://github.com/wonhongChang/jerrygram/actions/workflows/ci.yml)

Jerrygram is an Instagram-style social app built with React, ASP.NET Core, PostgreSQL, Redis, Blob Storage, Elasticsearch, Kafka, Logstash, Kibana, and a Node.js recommendation service.

## At A Glance

| Area | What Jerrygram demonstrates |
| --- | --- |
| Product flow | Register/login, feed, post upload, profile, search, notifications, saved posts, and explore recommendations |
| Backend | ASP.NET Core Web API with layered architecture, EF Core, Redis cache, Blob Storage, Elasticsearch, Kafka events, and JWT auth |
| Event analytics | Search, post, and user events flow through Kafka and Logstash/Kafka Connect into `jerrygram-events-*` indices |
| Recommendation | Node.js service ranks candidate posts with caption embeddings, Redis-backed cache, and cosine similarity |
| Java coverage | Separate Java 21 + Spring Boot backend kept as an alternate implementation and validated in CI |
| Quality gates | GitHub Actions build/test, .NET tests, Java smoke test, Node recommendation tests, React unit test, Playwright E2E |

The actively verified local path is:

- React web UI on `http://localhost:13000`
- ASP.NET Core Web API on `http://localhost:5018`
- Dockerized infrastructure for PostgreSQL, Redis, Elasticsearch, Kafka, Kafka UI, Logstash, Kibana, Kafka Connect, and the recommendation service

The repository also contains a Java/Spring Boot backend. The current web UI is wired to the .NET API by default.

## Architecture

![Jerrygram architecture](docs/assets/jerrygram-architecture.png)

Backend internals are documented in [docs/backend-architecture.md](docs/backend-architecture.md).

## Screenshots

![Jerrygram register screen](docs/assets/screenshots/jerrygram-register.png)

![Jerrygram feed screen](docs/assets/screenshots/jerrygram-feed.png)

![Jerrygram search trends screen](docs/assets/screenshots/jerrygram-search.png)

## Docs

- [Backend architecture](docs/backend-architecture.md)
- [Recommendation and Kafka evidence](docs/recommendation-and-kafka.md)
- [Elasticsearch index inventory](docs/elasticsearch-indexes.md)
- [Environment and secret setup](docs/env-and-secrets.md)
- [Seed data](infra/seed/README.md)

## Component READMEs

- [.NET backend](backend-dotnet/README.md)
- [Java backend](backend-java/README.md)
- [React frontend](frontend-react/README.md)
- [Recommendation service](jerrygram-recommend/README.md)

## Features

- JWT authentication with register, login, logout, and current-user loading
- Photo post creation with multipart upload
- Home feed, public posts, post detail, explore, profile, and search pages
- Likes, comments, follows, notifications, profile editing, and saved posts
- Redis-backed caching with in-memory fallback
- Elasticsearch-backed search and discovery
- Kafka event publishing from the .NET API
- Kafka to Logstash/Kafka Connect to Elasticsearch event pipeline
- Kibana support for `jerrygram-events-*`
- Node.js recommendation service that ranks candidates with caption embeddings and cosine similarity
- Playwright E2E tests for register, feed interaction, and Kafka-backed search trends

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

For secret handling, see [docs/env-and-secrets.md](docs/env-and-secrets.md).

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
cd backend-java
.\gradlew.bat test
```

```powershell
cd frontend-react
npm.cmd run test:ci
npm.cmd run e2e
```

```powershell
cd jerrygram-recommend
npm.cmd test
npm.cmd run lint
npm.cmd audit --omit=dev
```
