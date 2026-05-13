# Jerrygram Backend Architecture

Language: English | [한국어](backend-architecture.ko.md) | [日本語](backend-architecture.ja.md)

This document explains the active `backend-dotnet` architecture used by the current web UI. The Java/Spring backend remains in the repository as an alternate implementation, but the verified runtime path is the ASP.NET Core Web API.

![Jerrygram backend architecture](assets/backend-architecture.svg)

## Summary

Requests flow from `WebApi` to `Application` and `Domain`. Storage and external systems are handled by `Persistence` and `Infrastructure`. Write operations such as creating, updating, and deleting posts update PostgreSQL, Blob Storage, Elasticsearch, Redis cache, and the Kafka event pipeline.

## Layer Responsibilities

| Layer | Responsibility | Main files |
| --- | --- | --- |
| `WebApi` | HTTP entrypoint, auth, validation, middleware, DI | `Controllers`, `Middleware`, `ServiceExtensions.cs` |
| `Application` | CQRS handlers, DTOs, service interfaces, events | `Commands`, `Queries`, `Interfaces`, `Events` |
| `Domain` | Core entities, enums, value objects | `User`, `Post`, `Comment`, `PostCaption`, `PostVisibility` |
| `Persistence` | EF Core DbContext, repositories, migrations | `AppDbContext`, `Repositories` |
| `Infrastructure` | Redis, Kafka, Elasticsearch, Blob, JWT, recommendation client | `Services` |

## Main Flows

### Create Or Update Post

1. `PostController` receives a multipart/form-data request.
2. Web API request DTOs convert `IFormFile` into `Application.Common.UploadFile`.
3. Command handlers update the domain model.
4. `BlobService` stores the image and returns the public image URL.
5. EF Core repositories persist the data to PostgreSQL.
6. `ElasticService` indexes the searchable `posts` document.
7. Redis cache entries are invalidated by prefix.
8. Controllers enqueue events through `IEventPublisher`.
9. `KafkaEventDispatchService` publishes queued events to Kafka in the background.

### Read Feed

The feed includes the current user's own posts and followed-user posts. Own posts can use any visibility. Followed-user posts are limited to `Public` and `FollowersOnly`. `Private` posts are never exposed in another user's feed.

### Search And Popular Search

Search requests return Elasticsearch results and enqueue `SearchEvent` records. Kafka, Logstash, and Elasticsearch store those events under `jerrygram-events-search-YYYY.MM.DD`. `PopularSearchService` aggregates `searchTerm.keyword` from `jerrygram-events-*` and compares the latest six-hour window with the previous six-hour window for trending terms.

The active search trend path also includes `SearchTrendStreamProcessor`. It consumes Kafka `search-events`, updates Redis sorted-set buckets, and lets `PopularSearchService` read the Redis trend model first. Elasticsearch aggregation remains the fallback and audit trail.

## Event Pipeline

| Topic | Producer | Consumer | Purpose |
| --- | --- | --- | --- |
| `post-events` | .NET API | Logstash / Elasticsearch | Post create, like, comment, delete analytics |
| `user-events` | .NET API | Logstash / Elasticsearch | Register, login, follow, profile-view analytics |
| `search-events` | .NET API | Logstash / Elasticsearch | Popular and trending search analytics |
| `search-events` | .NET API | `SearchTrendStreamProcessor` / Redis | Near-real-time popular and trending search read model |

The controller does not wait on Kafka directly. `IEventPublisher` writes to a bounded channel, and a hosted service dispatches through `IEventService`. This keeps request latency and event delivery failure separate.

## Model Coverage

| Entity | Current coverage |
| --- | --- |
| `User` | username, email, password hash, profile image, created time, relations |
| `Post` | image URL, caption, visibility, author, comments, likes, saves, tags |
| `Comment` | content, author, post, created time |
| `Notification` | recipient, actor, type, post, message, read flag |
| Join models | follow, like, save, post-tag |

Recommended future fields include `UpdatedAt`, `DeletedAt`, `RowVersion`, `DisplayName`, `Bio`, `EmailVerifiedAt`, media metadata, moderation status, nested comments, and explicit recommendation interaction weights.

## Operational Notes

- Development can auto-apply EF migrations at startup.
- Production should run migrations separately from application startup.
- Runtime secrets should stay in local config or secret stores, not committed config.
- Elasticsearch documents are deleted when posts are deleted.
- If Kafka/ELK is unavailable, the API can keep core behavior through the no-op event service path.

## Verification

```powershell
$out = Join-Path $env:TEMP 'jerrygram-build-check-webapi'
dotnet build backend-dotnet/WebApi/WebApi.csproj -o $out
```
