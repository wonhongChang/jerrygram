# Jerrygram .NET Backend

Language: English | [한국어](README.ko.md) | [日本語](README.ja.md)

ASP.NET Core Web API for Jerrygram. This is the default backend used by the React web UI.

## Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Redis cache with in-memory fallback
- Elasticsearch search
- Kafka event publishing
- Azure-compatible Blob Storage support
- xUnit tests

## Projects

```text
backend-dotnet/
|- Application/            commands, queries, DTOs, interfaces
|- Domain/                 entities, constants, value objects
|- Persistence/            EF Core DbContext, migrations, repositories
|- Infrastructure/         auth, cache, blob, search, Kafka, recommendation clients
|- WebApi/                 controllers, middleware, validators, startup
|- Domain.Tests/           domain behavior tests
|- Infrastructure.Tests/   infrastructure query tests
\- Jerrygram/              Visual Studio solution
```

## Local Configuration

Copy the example config before running locally:

```powershell
Copy-Item WebApi/appsettings.example.json WebApi/appsettings.json
```

The checked-in Docker config uses the adjusted local ports from the root compose files.

## Commands

From the repository root:

```powershell
dotnet restore backend-dotnet/WebApi/WebApi.csproj
dotnet build backend-dotnet/WebApi/WebApi.csproj --configuration Release
dotnet test backend-dotnet/Domain.Tests/Domain.Tests.csproj --configuration Release
dotnet test backend-dotnet/Infrastructure.Tests/Infrastructure.Tests.csproj --configuration Release
```

Run the API:

```powershell
dotnet run --project backend-dotnet/WebApi/WebApi.csproj --urls http://localhost:5018
```

## API Areas

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/posts`
- `POST /api/posts`
- `GET /api/posts/feed`
- `GET /api/explore`
- `GET /api/search`
- `GET /api/search/popular`
- `GET /api/search/popular/trending`
- `GET /api/users/me`
- `GET /api/users/{username}`
- `GET /api/notifications`

## Notes

- `bin/`, `obj/`, `*.user`, local `appsettings.json`, and `appsettings.Development.json` are ignored and should not be committed.
- `appsettings.example.json` is the shared local template.
- Kafka events are published by the API and consumed into Elasticsearch through the local analytics pipeline.
