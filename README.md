# Jerrygram

Language: English | [한국어](README.ko.md) | [日本語](README.ja.md)

Jerrygram is a full-stack Instagram-style social app used to demonstrate a production-minded local stack: React, ASP.NET Core, PostgreSQL, Redis, local/Azure-compatible Blob Storage, Elasticsearch, Kafka, Logstash, Kibana, and a Node.js recommendation service.

The actively verified local development path is:

- React web UI on `http://localhost:13000`
- ASP.NET Core Web API on `http://localhost:5018`
- Dockerized infrastructure for PostgreSQL, Redis, Elasticsearch, Kafka, Kafka UI, Logstash, Kibana, Kafka Connect, and the recommendation service

The repository also contains a Java/Spring backend as an alternate implementation, but the current web UI is wired to the .NET API by default.

## Architecture

![Jerrygram architecture](docs/assets/jerrygram-architecture.png)

Backend internals are documented in [docs/backend-architecture.md](docs/backend-architecture.md).

## Screenshots

![Jerrygram register screen](docs/assets/screenshots/jerrygram-register.png)

![Jerrygram feed screen](docs/assets/screenshots/jerrygram-feed.png)

![Jerrygram search trends screen](docs/assets/screenshots/jerrygram-search.png)

## Evidence And Docs

- [Backend architecture](docs/backend-architecture.md)
- [Recommendation and Kafka evidence](docs/recommendation-and-kafka.md)
- [Elasticsearch index inventory](docs/elasticsearch-indexes.md)
- [Environment and secret setup](docs/env-and-secrets.md)
- [Seed data](infra/seed/README.md)

## Features

- JWT authentication with register, login, logout, and current-user loading
- Photo post creation with multipart upload
- Home feed, public posts, post detail, explore, profile, and search pages
- Likes, comments, follows, notifications, profile editing, and saved posts
- Redis-backed caching with in-memory fallback
- Elasticsearch-backed search and discovery
- Kafka event publishing from the .NET API
- Kafka to Logstash/Kafka Connect to Elasticsearch event pipeline for analytics
- Kibana support for `jerrygram-events-*`
- Node.js recommendation service that ranks candidates with caption embeddings and cosine similarity
- Playwright E2E tests for register, feed interaction, and Kafka-backed search trends

## Local Ports

Jerrygram uses non-default host ports to avoid collisions with other Docker projects.

| Service | URL / host port |
| --- | --- |
| React web UI | `http://localhost:13000` |
| ASP.NET Core API | `http://localhost:5018` |
| Recommendation service | `http://localhost:13001` |
| PostgreSQL | `localhost:15433` |
| Redis | `localhost:16380` |
| Elasticsearch | `http://localhost:19200` |
| Elasticsearch transport | `localhost:19300` |
| Kafka | `localhost:19092` |
| Kafka JMX | `localhost:19997` |
| Kafka UI | `http://localhost:18081` |
| Kibana | `http://localhost:15601` |
| Logstash Beats | `localhost:15044` |
| Logstash API | `http://localhost:19600` |
| Kafka Connect | `http://localhost:18083` |

These defaults are configurable with `JG_*` environment variables in the compose files.

## Project Structure

```text
jerrygram/
  backend-dotnet/                 ASP.NET Core API
    Domain/                       Domain entities and enums
    Application/                  CQRS commands, queries, handlers, DTOs
    Infrastructure/               Redis, Kafka, Elasticsearch, Blob, JWT
    Persistence/                  EF Core DbContext, repositories, migrations
    WebApi/                       Controllers, middleware, app configuration
  backend-java/                   Alternate Spring Boot API implementation
  frontend-react/                 React + TypeScript web app
  jerrygram-recommend/            Node.js recommendation service
  infra/                          Setup scripts and seed data
  logstash/                       Logstash pipeline configuration
  docker-compose.yml              Core infrastructure and recommendation service
  docker-compose.kafka-elk-extended.yml
                                  Elasticsearch, Kafka, Kibana, Logstash stack
```

## Prerequisites

- Docker Desktop
- .NET SDK 8 or newer
- Node.js and npm
- PowerShell on Windows

Optional:

- `dotnet-ef` for manual migration work
- Azure Storage or Azurite-compatible Blob settings if you want real image storage instead of fallback behavior

## Environment

Copy the example files before running locally:

```powershell
Copy-Item .env.example .env
Copy-Item backend-dotnet/WebApi/appsettings.example.json backend-dotnet/WebApi/appsettings.json
Copy-Item frontend-react/.env.example frontend-react/.env
Copy-Item jerrygram-recommend/.env.example jerrygram-recommend/.env
Copy-Item backend-java/.env.example backend-java/.env
```

See [docs/env-and-secrets.md](docs/env-and-secrets.md) for secret and port details.

## Start The Local Stack

Start Docker infrastructure:

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka-elk-extended.yml up -d
```

Apply .NET database migrations when needed:

```powershell
dotnet ef database update `
  --project backend-dotnet/Persistence/Persistence.csproj `
  --startup-project backend-dotnet/WebApi/WebApi.csproj
```

Run the .NET API:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project backend-dotnet/WebApi/WebApi.csproj --urls http://localhost:5018
```

Run the React app:

```powershell
cd frontend-react
npm install
npm start
```

Open `http://localhost:13000`.

## Health Checks

```powershell
docker ps
Invoke-WebRequest http://localhost:13000 -UseBasicParsing
Invoke-WebRequest http://localhost:5018/api/posts?page=1&pageSize=3 -UseBasicParsing
Invoke-WebRequest http://localhost:13001/health -UseBasicParsing
Invoke-RestMethod http://localhost:19200/_cluster/health
Invoke-WebRequest http://localhost:15601/api/status -UseBasicParsing
Invoke-WebRequest http://localhost:19600/_node/stats -UseBasicParsing
```

Kafka topics:

```powershell
docker exec jg-kafka kafka-topics --bootstrap-server kafka:29092 --list
```

Expected Jerrygram event topics:

```text
post-events
user-events
search-events
popular-searches
```

Elasticsearch event indices:

```powershell
Invoke-RestMethod "http://localhost:19200/_cat/indices/jerrygram-events-*?format=json&h=index,docs.count,health,status"
```

## Kafka And ELK Evidence

The .NET API publishes events to Kafka topics such as `post-events`, `user-events`, `search-events`, and `popular-searches`. Event data is indexed into Elasticsearch as daily `jerrygram-events-*` indices and can be inspected in Kibana.

![Kafka UI topics](docs/assets/screenshots/kafka-ui-topics.png)

![Kibana event indices](docs/assets/screenshots/kibana-indices.png)

More detail is available in [docs/recommendation-and-kafka.md](docs/recommendation-and-kafka.md).

## Verified UI Flow

The local UI has been checked against the .NET API with this flow:

1. Register a new user from `/register`.
2. Create a post with an image and caption.
3. Verify the post appears on the home feed.
4. Save the post from the feed.
5. Verify it appears on the profile `Saved` tab.
6. Open the detail page and confirm the saved state.
7. Click the comments action and confirm the comment input receives focus.
8. Search for the created user.
9. Delete the post from the feed options menu.
10. Confirm the deleted post returns `404` from the API.

## Build And Test

.NET backend:

```powershell
dotnet build backend-dotnet/WebApi/WebApi.csproj
```

React frontend:

```powershell
cd frontend-react
npm run test:ci
npm run build
npm run e2e
```

Screenshot capture:

```powershell
cd frontend-react
npm run screenshots
```

GitHub Actions runs build/test checks for the .NET backend, Java backend, recommendation service, and React frontend. It does not deploy the app.

## Development Notes

- Elasticsearch can show `yellow` health in this single-node local setup because replicas are unassigned. That is expected for local development.
- Existing seeded image URLs may return Blob `404` responses if the remote blob no longer exists. The UI uses fallback image rendering for those cases.
- Redis cache invalidation covers feed, public posts, saved posts, detail pages, and explore data after post and interaction changes.
- The home feed includes the current user's own posts as well as followed-user posts.
- If another project is using common ports like `3000`, `6379`, `8080`, or `9200`, keep Jerrygram on the `JG_*` ports listed above.

## License

This project is licensed under the MIT License.
