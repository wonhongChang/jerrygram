# Jerrygram バックエンドアーキテクチャ

Language: [English](backend-architecture.md) | [한국어](backend-architecture.ko.md) | 日本語

この文書は、現在の Web UI が利用している `backend-dotnet` の構成を説明します。Java/Spring バックエンドは代替実装として残っていますが、検証済みの実行経路は ASP.NET Core Web API です。

![Jerrygram backend architecture](assets/backend-architecture.svg)

## 概要

リクエストは `WebApi` から `Application`、`Domain` へ流れます。保存先や外部システムとの連携は `Persistence` と `Infrastructure` が担当します。投稿の作成、更新、削除では PostgreSQL、Blob Storage、Elasticsearch、Redis cache、Kafka event pipeline が更新されます。

## レイヤー責務

| Layer | Responsibility | Main files |
| --- | --- | --- |
| `WebApi` | HTTP 入口、認証、検証、middleware、DI | `Controllers`, `Middleware`, `ServiceExtensions.cs` |
| `Application` | CQRS handler、DTO、service interface、event | `Commands`, `Queries`, `Interfaces`, `Events` |
| `Domain` | Core entity、enum、value object | `User`, `Post`, `Comment`, `PostCaption`, `PostVisibility` |
| `Persistence` | EF Core DbContext、repository、migration | `AppDbContext`, `Repositories` |
| `Infrastructure` | Redis、Kafka、Elasticsearch、Blob、JWT、推薦 client | `Services` |

## 主な流れ

### 投稿の作成/更新

1. `PostController` が multipart/form-data request を受け取ります。
2. Web API request DTO が `IFormFile` を `Application.Common.UploadFile` に変換します。
3. Command handler が domain model を更新します。
4. `BlobService` が画像を保存し、公開画像 URL を返します。
5. EF Core repository が PostgreSQL に保存します。
6. `ElasticService` が検索用の `posts` document を index します。
7. 関連 Redis cache を prefix で無効化します。
8. Controller は `IEventPublisher` で event を queue に入れます。
9. `KafkaEventDispatchService` が background で Kafka topic に発行します。

### フィード取得

フィードには現在ユーザー自身の投稿と、フォロー中ユーザーの投稿が含まれます。自身の投稿はすべての visibility が対象です。フォロー中ユーザーの投稿は `Public` と `FollowersOnly` のみ表示されます。`Private` 投稿は他ユーザーのフィードには表示されません。

### 検索と人気検索語

検索 request は Elasticsearch の結果を返し、`SearchEvent` を queue に入れます。Kafka、Logstash、Elasticsearch は event を `jerrygram-events-search-YYYY.MM.DD` に保存します。`PopularSearchService` は `jerrygram-events-*` の `searchTerm.keyword` を aggregation し、直近 6 時間とその前の 6 時間を比較して trending term を計算します。

## イベントパイプライン

| Topic | Producer | Consumer | Purpose |
| --- | --- | --- | --- |
| `post-events` | .NET API | Logstash / Elasticsearch | 投稿作成、いいね、コメント、削除の分析 |
| `user-events` | .NET API | Logstash / Elasticsearch | 登録、ログイン、フォロー、プロフィール閲覧の分析 |
| `search-events` | .NET API | Logstash / Elasticsearch | 人気/トレンド検索語の分析 |

Controller は Kafka 発行を直接待ちません。`IEventPublisher` が bounded channel に event を入れ、hosted service が `IEventService` 経由で発行します。これにより、request latency と event delivery failure を分離できます。

## モデルカバレッジ

| Entity | Current coverage |
| --- | --- |
| `User` | username, email, password hash, profile image, created time, relations |
| `Post` | image URL, caption, visibility, author, comments, likes, saves, tags |
| `Comment` | content, author, post, created time |
| `Notification` | recipient, actor, type, post, message, read flag |
| Join models | follow, like, save, post-tag |

今後追加を検討する field は `UpdatedAt`、`DeletedAt`、`RowVersion`、`DisplayName`、`Bio`、`EmailVerifiedAt`、media metadata、moderation status、nested comment、recommendation interaction weight です。

## 運用メモ

- 開発環境では起動時に EF migration を自動適用できます。
- 本番環境では migration を application startup から分離する方が安全です。
- runtime secret は local config や secret store に置き、commit しません。
- 投稿削除時には Elasticsearch document も削除します。
- Kafka/ELK が利用できなくても、no-op event service により core API behavior は維持できます。

## 検証

```powershell
$out = Join-Path $env:TEMP 'jerrygram-build-check-webapi'
dotnet build backend-dotnet/WebApi/WebApi.csproj -o $out
```
