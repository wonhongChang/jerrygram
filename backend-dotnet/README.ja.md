# Jerrygram .NET バックエンド

Language: [English](README.md) | [한국어](README.ko.md) | 日本語

Jerrygram の ASP.NET Core Web API です。React Web UI がデフォルトで利用するバックエンドです。

## 技術スタック

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Redis キャッシュとインメモリ fallback
- Elasticsearch 検索
- Kafka イベント発行
- Azure 互換 Blob Storage 対応
- xUnit テスト

## プロジェクト構成

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

## ローカル設定

実行前にサンプル設定をコピーします。

```powershell
Copy-Item WebApi/appsettings.example.json WebApi/appsettings.json
```

チェックイン済みの Docker 設定は、ルートの compose ファイルで調整したローカルポートを使います。

## コマンド

リポジトリルートから実行します。

```powershell
dotnet restore backend-dotnet/WebApi/WebApi.csproj
dotnet build backend-dotnet/WebApi/WebApi.csproj --configuration Release
dotnet test backend-dotnet/Domain.Tests/Domain.Tests.csproj --configuration Release
dotnet test backend-dotnet/Infrastructure.Tests/Infrastructure.Tests.csproj --configuration Release
```

API 起動:

```powershell
dotnet run --project backend-dotnet/WebApi/WebApi.csproj --urls http://localhost:5018
```

## API 領域

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

## メモ

- `bin/`, `obj/`, `*.user`, ローカル `appsettings.json`, `appsettings.Development.json` は ignore 対象で、コミットしません。
- `appsettings.example.json` は共有ローカルテンプレートです。
- Kafka イベントは API から発行され、ローカル analytics パイプラインを通じて Elasticsearch に保存されます。
