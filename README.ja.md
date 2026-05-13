# Jerrygram

Language: [English](README.md) | [한국어](README.ko.md) | 日本語

[![Build and test](https://github.com/wonhongChang/jerrygram/actions/workflows/ci.yml/badge.svg)](https://github.com/wonhongChang/jerrygram/actions/workflows/ci.yml)

Jerrygram は React、ASP.NET Core、PostgreSQL、Redis、Blob Storage、Elasticsearch、Kafka、Logstash、Kibana、Node.js レコメンドサービスで構成された Instagram 風のソーシャルアプリです。

## 一目でわかる概要

| 領域 | Jerrygram で示していること |
| --- | --- |
| Product flow | 登録/ログイン、フィード、投稿アップロード、プロフィール、検索、通知、保存済み投稿、探索レコメンド |
| Backend | layered architecture の ASP.NET Core Web API、EF Core、Redis cache、Blob Storage、Elasticsearch、Kafka event、JWT auth |
| Event analytics | 検索/投稿/ユーザー event が Kafka に流れ、検索 event は Redis read model に stream processing されて Search 画面の Live trends パネルを動かし、`jerrygram-events-*` index にも保存される |
| Recommendation | Node.js service が caption embedding、Redis cache、cosine similarity で候補投稿を並べる |
| Java coverage | Java 21 + Spring Boot backend を代替実装として保持し、CI で検証 |
| Quality gates | GitHub Actions build/test、.NET tests、Java smoke test、Node recommendation tests、React unit test、Playwright E2E |

現在検証している基本の実行経路は次のとおりです。

- React Web UI: `http://localhost:13000`
- ASP.NET Core Web API: `http://localhost:5018`
- Docker インフラ: PostgreSQL、Redis、Elasticsearch、Kafka、Kafka UI、Logstash、Kibana、Kafka Connect、レコメンドサービス

リポジトリには Java/Spring Boot バックエンドも含まれています。現在の Web UI はデフォルトで .NET API に接続します。

## アーキテクチャ

![Jerrygram architecture](docs/assets/jerrygram-architecture.png)

バックエンド内部構造は [docs/backend-architecture.ja.md](docs/backend-architecture.ja.md) にまとめています。

## デモ画面

![登録画面](docs/assets/screenshots/jerrygram-register.png)

![フィード画面](docs/assets/screenshots/jerrygram-feed.png)

![リアルタイム検索トレンド画面](docs/assets/screenshots/jerrygram-search.png)

## ドキュメント

- [バックエンドアーキテクチャ](docs/backend-architecture.ja.md)
- [レコメンドと Kafka の証跡](docs/recommendation-and-kafka.ja.md)
- [Kafka stream processing](docs/stream-processing.ja.md)
- [Elasticsearch インデックス一覧](docs/elasticsearch-indexes.ja.md)
- [環境変数とシークレット設定](docs/env-and-secrets.ja.md)
- [シードデータ](infra/seed/README.ja.md)

## コンポーネント README

- [.NET バックエンド](backend-dotnet/README.ja.md)
- [Java バックエンド](backend-java/README.ja.md)
- [React フロントエンド](frontend-react/README.ja.md)
- [レコメンドサービス](jerrygram-recommend/README.ja.md)

## 機能

- 登録、ログイン、ログアウト、現在ユーザー取得を含む JWT 認証
- multipart アップロードによる写真投稿作成
- ホームフィード、公開投稿、投稿詳細、探索、プロフィール、検索画面
- いいね、コメント、フォロー、通知、プロフィール編集、保存済み投稿
- Redis キャッシュとインメモリ fallback
- Elasticsearch による検索と探索
- .NET API からの Kafka イベント発行
- Redis にほぼリアルタイムの検索トレンドを蓄積する Kafka consumer stream processing
- Search 画面の Live trends パネルが stream processing read model を 10 秒ごとに再取得
- Kafka から Logstash/Kafka Connect を経由して Elasticsearch に保存するイベントパイプライン
- `jerrygram-events-*` を確認できる Kibana 構成
- キャプション embedding と cosine similarity で候補投稿を並べる Node.js レコメンドサービス
- 登録、フィード操作、Kafka ベースの検索トレンドを検証する Playwright E2E テスト

## ローカルポート

Jerrygram は他の Docker プロジェクトと衝突しにくいようにホストポートを調整しています。

| サービス | URL / ホストポート |
| --- | --- |
| React Web UI | `http://localhost:13000` |
| ASP.NET Core API | `http://localhost:5018` |
| レコメンドサービス | `http://localhost:13001` |
| PostgreSQL | `localhost:15433` |
| Redis | `localhost:16380` |
| Elasticsearch | `http://localhost:19200` |
| Kafka | `localhost:19092` |
| Kafka UI | `http://localhost:18081` |
| Kibana | `http://localhost:15601` |
| Logstash API | `http://localhost:19600` |
| Kafka Connect | `http://localhost:18083` |

## セットアップ

```powershell
Copy-Item .env.example .env
Copy-Item backend-dotnet/WebApi/appsettings.example.json backend-dotnet/WebApi/appsettings.json
Copy-Item frontend-react/.env.example frontend-react/.env
Copy-Item jerrygram-recommend/.env.example jerrygram-recommend/.env
Copy-Item backend-java/.env.example backend-java/.env
```

シークレットの扱いは [docs/env-and-secrets.ja.md](docs/env-and-secrets.ja.md) を参照してください。

## Docker インフラの起動

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka-elk-extended.yml up -d
```

## .NET API の起動

```powershell
dotnet run --project backend-dotnet/WebApi/WebApi.csproj --urls http://localhost:5018
```

## React Web の起動

```powershell
cd frontend-react
npm install
npm start
```

現在のフロントエンドは Vite ではなく Create React App(`react-scripts`) です。検証済みのローカル実行では React dev server を Docker の外で起動し、Docker はインフラとバックエンド周辺サービスに集中させています。この方が HMR が速く、デバッグも単純で、他の Docker プロジェクトとのポート衝突も減らせます。デプロイ用パッケージングやワンコマンドデモが必要になったら、フロントエンド用 Docker image を別途追加できます。

## シードと証跡

```powershell
powershell -ExecutionPolicy Bypass -File .\infra\seed\seed-jerrygram.ps1
powershell -ExecutionPolicy Bypass -File .\infra\seed\verify-jerrygram-demo.ps1
```

## 検証

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
