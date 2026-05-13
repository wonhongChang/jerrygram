# Jerrygram バックエンドアーキテクチャ

Language: [English](backend-architecture.md) | [한국어](backend-architecture.ko.md) | 日本語

この文書は、現在の Web UI が利用する `backend-dotnet` アーキテクチャを説明します。Java/Spring バックエンドは代替実装としてリポジトリに残していますが、検証済みの基本実行経路は ASP.NET Core Web API です。

![Jerrygram backend architecture](assets/backend-architecture.svg)

## 概要

リクエストは `WebApi` から `Application` と `Domain` に流れます。ストレージと外部システムは `Persistence` と `Infrastructure` が担当します。投稿の作成、更新、削除などの書き込み処理は PostgreSQL、Blob Storage、Elasticsearch、Redis cache、Kafka event pipeline を更新します。

## レイヤー責務

| レイヤー | 責務 | 主なファイル |
| --- | --- | --- |
| `WebApi` | HTTP エントリ、認証、検証、ミドルウェア、DI | `Controllers`, `Middleware`, `ServiceExtensions.cs` |
| `Application` | CQRS handler、DTO、service interface、event | `Commands`, `Queries`, `Interfaces`, `Events` |
| `Domain` | Core entity、enum、value object | `User`, `Post`, `Comment`, `PostCaption`, `PostVisibility` |
| `Persistence` | EF Core DbContext、repository、migration | `AppDbContext`, `Repositories` |
| `Infrastructure` | Redis、Kafka、Elasticsearch、Blob、JWT、recommendation client | `Services` |

## 主なフロー

### 投稿作成または更新

1. `PostController` が multipart/form-data request を受け取ります。
2. Web API request DTO が `IFormFile` を `Application.Common.UploadFile` に変換します。
3. Command handler が domain model を更新します。
4. `BlobService` が画像を保存し、公開画像 URL を返します。
5. EF Core repository が PostgreSQL に保存します。
6. `ElasticService` が検索用の `posts` document を index します。
7. Redis cache entry を prefix で無効化します。
8. Controller が `IEventPublisher` で event を queue に入れます。
9. `KafkaEventDispatchService` が background で Kafka に発行します。

### フィード読み込み

フィードには現在ユーザー自身の投稿とフォロー中ユーザーの投稿が含まれます。自身の投稿はすべての visibility を使えます。フォロー中ユーザーの投稿は `Public` と `FollowersOnly` のみ表示されます。`Private` 投稿は他ユーザーのフィードには表示されません。

### 検索と人気検索語

検索 request は Elasticsearch 結果を返し、`SearchEvent` を queue に入れます。Kafka、Logstash、Elasticsearch は event を `jerrygram-events-search-YYYY.MM.DD` に保存します。`PopularSearchService` は `jerrygram-events-*` の `searchTerm.keyword` を集計し、直近 6 時間と前の 6 時間を比較して trending term を計算します。

現在の検索トレンド経路には `SearchTrendStreamProcessor` も含まれます。この processor は Kafka `search-events` を consume し、Redis sorted-set bucket を更新します。`PopularSearchService` は Redis trend model を先に読み、Elasticsearch aggregation を fallback と audit trail として使います。

## イベントパイプライン

| Topic | Producer | Consumer | 目的 |
| --- | --- | --- | --- |
| `post-events` | .NET API | Logstash / Elasticsearch | 投稿作成、いいね、コメント、削除 analytics |
| `user-events` | .NET API | Logstash / Elasticsearch | 登録、ログイン、フォロー、プロフィール閲覧 analytics |
| `search-events` | .NET API | Logstash / Elasticsearch | 人気/トレンド検索 analytics |
| `search-events` | .NET API | `SearchTrendStreamProcessor` / Redis | ほぼリアルタイムの人気/トレンド検索 read model |

Controller は Kafka を直接待ちません。`IEventPublisher` が bounded channel に event を書き込み、hosted service が `IEventService` 経由で dispatch します。これにより request latency と event delivery failure を分離します。

## モデル範囲

| Entity | 現在の範囲 |
| --- | --- |
| `User` | username, email, password hash, profile image, created time, relations |
| `Post` | image URL, caption, visibility, author, comments, likes, saves, tags |
| `Comment` | content, author, post, created time |
| `Notification` | recipient, actor, type, post, message, read flag |
| Join models | follow, like, save, post-tag |

今後追加するとよい項目は `UpdatedAt`, `DeletedAt`, `RowVersion`, `DisplayName`, `Bio`, `EmailVerifiedAt`, media metadata, moderation status, nested comments, 明示的な recommendation interaction weight です。

## 運用メモ

- 開発環境では起動時に EF migration を自動適用できます。
- 本番環境では migration をアプリ起動から分離して実行するのが望ましいです。
- Runtime secret は local config または secret store に置き、コミットしません。
- 投稿削除時には Elasticsearch document も削除されます。
- Kafka/ELK が利用できない場合でも no-op event service 経路で core API behavior は維持できます。

## 検証

```powershell
$out = Join-Path $env:TEMP 'jerrygram-build-check-webapi'
dotnet build backend-dotnet/WebApi/WebApi.csproj -o $out
```
