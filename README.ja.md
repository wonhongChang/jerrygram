# Jerrygram

言語: [English](README.md) | [한국어](README.ko.md) | 日本語

Jerrygram は、React、ASP.NET Core、PostgreSQL、Redis、Azure Blob Storage 連携、Elasticsearch、Kafka、Logstash、Kibana、Node.js レコメンドサービスを組み合わせた Instagram 風のフルスタックソーシャルアプリです。ローカル環境でも本番に近い構成を確認できるように設計されています。

現在検証済みのローカル開発経路は次の通りです。

- React Web UI: `http://localhost:13000`
- ASP.NET Core Web API: `http://localhost:5018`
- PostgreSQL、Redis、Elasticsearch、Kafka、Kafka UI、Logstash、Kibana、Kafka Connect、レコメンドサービスは Docker で起動

このリポジトリには Java/Spring バックエンドも代替実装として含まれていますが、現在の Web UI はデフォルトで .NET API に接続します。

## アーキテクチャ概要

![Jerrygram アーキテクチャ](docs/assets/jerrygram-architecture.png)

## 現在の機能

- JWT ベースの登録、ログイン、ログアウト、現在ユーザーの読み込み
- multipart アップロードによる写真投稿作成
- ホームフィード、公開投稿、投稿詳細、探索、プロフィール、検索ページ
- いいね、コメント、フォロー、通知、プロフィール編集
- 保存/ブックマーク投稿とプロフィールの `Saved` タブ
- Redis ベースのキャッシュとインメモリ fallback
- Elasticsearch ベースの検索と探索
- .NET API からの Kafka イベント発行
- Kafka から Logstash を経由して Elasticsearch へ送るイベント分析パイプライン
- `jerrygram-events-*` 用 Kibana data view のサポート
- 独立した Node.js レコメンドサービス

## ローカルポート

Jerrygram は他の Docker プロジェクトとの衝突を避けるため、一般的なデフォルトポートではなく専用のホストポートを使用します。

| サービス | URL / ホストポート |
| --- | --- |
| React Web UI | `http://localhost:13000` |
| ASP.NET Core API | `http://localhost:5018` |
| レコメンドサービス | `http://localhost:13001` |
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

これらのデフォルト値は compose ファイル内の `JG_*` 環境変数で変更できます。

## プロジェクト構成

```text
jerrygram/
  backend-dotnet/                 ASP.NET Core API
    Domain/                       ドメインエンティティと enum
    Application/                  CQRS command, query, handler, DTO
    Infrastructure/               Redis, Kafka, Elasticsearch, Blob, JWT
    Persistence/                  EF Core DbContext, repository, migration
    WebApi/                       controller, middleware, app configuration
  backend-java/                   代替 Spring Boot API 実装
  frontend-react/                 React + TypeScript Web アプリ
  jerrygram-recommend/            Node.js レコメンドサービス
  infra/                          Elasticsearch, Kafka, Kibana 設定スクリプト
  logstash/                       Logstash pipeline 設定
  docker-compose.yml              コアインフラとレコメンドサービス
  docker-compose.kafka-elk-extended.yml
                                  Elasticsearch, Kafka, Kibana, Logstash スタック
```

## 必要条件

- Docker Desktop
- .NET SDK 8 以上
- Node.js と npm
- Windows PowerShell

任意:

- 手動 migration 作業用の `dotnet-ef`
- fallback 画像ではなく実際の画像保存を使う場合は、Azure Storage または Azurite 互換 Blob 設定

## 環境設定

リポジトリルートに `.env` を作成または更新します。ローカル compose スタックでは、最低限 Redis とレコメンドサービス関連の値が必要です。

```env
REDIS_PASSWORD=your-local-redis-password
OPENAI_API_KEY=your-openai-api-key
JG_RECOMMEND_PORT=13001
JG_POSTGRES_PORT=15433
JG_REDIS_PORT=16380
JG_ELASTICSEARCH_PORT=19200
JG_KAFKA_PORT=19092
JG_KAFKA_UI_PORT=18081
JG_KIBANA_PORT=15601
JG_KAFKA_CONNECT_PORT=18083
```

React アプリは .NET API を参照するように設定します。

```env
# frontend-react/.env
REACT_APP_API_URL=http://localhost:5018/api
PORT=13000
```

## ローカルスタックの起動

Docker インフラを起動します。

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka-elk-extended.yml up -d
```

必要に応じて .NET データベース migration を適用します。

```powershell
dotnet ef database update `
  --project backend-dotnet/Persistence/Persistence.csproj `
  --startup-project backend-dotnet/WebApi/WebApi.csproj
```

.NET API を起動します。

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project backend-dotnet/WebApi/WebApi.csproj --urls http://localhost:5018
```

React アプリを起動します。

```powershell
cd frontend-react
npm install
npm start
```

アプリを開きます。

```text
http://localhost:13000
```

## ヘルスチェック

よく使うローカル確認コマンドです。

```powershell
docker ps
Invoke-WebRequest http://localhost:13000 -UseBasicParsing
Invoke-WebRequest http://localhost:5018/api/posts?page=1&pageSize=3 -UseBasicParsing
Invoke-WebRequest http://localhost:13001/health -UseBasicParsing
Invoke-RestMethod http://localhost:19200/_cluster/health
Invoke-WebRequest http://localhost:15601/api/status -UseBasicParsing
Invoke-WebRequest http://localhost:19600/_node/stats -UseBasicParsing
```

Kafka トピック:

```powershell
docker exec jg-kafka kafka-topics --bootstrap-server kafka:29092 --list
```

期待される Jerrygram イベントトピック:

```text
post-events
user-events
search-events
popular-searches
```

Elasticsearch イベントインデックス:

```powershell
Invoke-RestMethod "http://localhost:19200/_cat/indices/jerrygram-events-*?format=json&h=index,docs.count,health,status"
```

## 検証済み UI フロー

現在のローカル UI は .NET API を対象に次の流れで検証済みです。

1. `/register` から新規ユーザーを登録
2. 画像とキャプション付きの投稿を作成
3. 投稿がホームフィードに表示されることを確認
4. フィードから投稿を保存
5. プロフィールの `Saved` タブに表示されることを確認
6. 詳細ページで保存状態を確認
7. コメントアクションをクリックし、コメント入力欄にフォーカスが移ることを確認
8. 作成したユーザーが検索できることを確認
9. フィードのオプションメニューから投稿を削除
10. 削除された投稿が API で `404` を返すことを確認

## Kafka と ELK

.NET API は `post-events`、`user-events`、`search-events`、`popular-searches` などの Kafka トピックへイベントを発行します。

Logstash は Kafka イベントを消費し、日次の Elasticsearch インデックスへ書き込みます。

```text
jerrygram-events-post-YYYY.MM.DD
jerrygram-events-user-YYYY.MM.DD
jerrygram-events-search-YYYY.MM.DD
```

Kibana は次の URL で利用できます。

```text
http://localhost:15601
```

`jerrygram-events-*` には `Jerrygram Events` data view を使用します。

## 開発メモ

- 単一ノードのローカル環境では replica が割り当てられないため、Elasticsearch の状態が `yellow` になる場合があります。ローカル開発では想定内です。
- 既存 seed 画像 URL は、リモート Blob が存在しない場合に `404` を返すことがあります。UI はその場合 fallback 画像を表示します。
- 投稿の作成、更新、削除、いいね、いいね解除、保存、保存解除の後は、フィード、公開投稿、保存投稿、詳細ページ、探索データのキャッシュを広めに無効化します。
- ホームフィードにはフォロー中ユーザーの投稿だけでなく、現在ユーザー自身の投稿も含まれます。
- 他のプロジェクトが `3000`、`6379`、`8080`、`9200` などの一般的なポートを使っている場合は、上記の `JG_*` ポートを維持するのがおすすめです。

## ビルドコマンド

バックエンド:

```powershell
dotnet build backend-dotnet/WebApi/WebApi.csproj
```

フロントエンド:

```powershell
cd frontend-react
npm run build
```

## ライセンス

このプロジェクトは MIT ライセンスのもとで提供されています。
