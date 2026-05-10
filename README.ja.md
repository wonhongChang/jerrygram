# Jerrygram

Language: [English](README.md) | [한국어](README.ko.md) | 日本語

Jerrygram は Instagram 風のフルスタック SNS アプリです。React、ASP.NET Core、PostgreSQL、Redis、ローカル/Azure 互換 Blob Storage、Elasticsearch、Kafka、Logstash、Kibana、Node.js 推薦サービスを、ローカルで再現できる構成としてまとめています。

現在確認済みの基本構成は次の通りです。

- React Web UI: `http://localhost:13000`
- ASP.NET Core Web API: `http://localhost:5018`
- PostgreSQL、Redis、Elasticsearch、Kafka、Kafka UI、Logstash、Kibana、Kafka Connect、推薦サービスは Docker で起動

リポジトリには Java/Spring の代替バックエンドも含まれていますが、現在の Web UI は標準で .NET API に接続します。

## アーキテクチャ

![Jerrygram architecture](docs/assets/jerrygram-architecture.png)

バックエンド内部の構造は [docs/backend-architecture.md](docs/backend-architecture.md) にまとめています。

## スクリーンショット

![Register screen](docs/assets/screenshots/jerrygram-register.png)

![Feed screen](docs/assets/screenshots/jerrygram-feed.png)

![Search trends screen](docs/assets/screenshots/jerrygram-search.png)

## ドキュメントと証跡

- [Backend architecture](docs/backend-architecture.ja.md)
- [Recommendation and Kafka evidence](docs/recommendation-and-kafka.ja.md)
- [Elasticsearch index inventory](docs/elasticsearch-indexes.ja.md)
- [Environment and secret setup](docs/env-and-secrets.ja.md)
- [Seed data](infra/seed/README.ja.md)

## 主な機能

- JWT ベースの登録、ログイン、ログアウト、現在ユーザー読み込み
- multipart 画像アップロードによる投稿作成
- ホームフィード、公開投稿、投稿詳細、探索、プロフィール、検索画面
- いいね、コメント、フォロー、通知、プロフィール編集、保存済み投稿
- Redis キャッシュとインメモリ fallback
- Elasticsearch ベースの検索と探索
- .NET API から Kafka へのイベント発行
- Kafka から Logstash/Kafka Connect を経由して Elasticsearch に保存する分析パイプライン
- `jerrygram-events-*` を Kibana で確認できる構成
- キャプション embedding と cosine similarity による Node.js 推薦サービス
- 登録、フィード操作、Kafka ベース検索トレンドを検証する Playwright E2E テスト

## ローカルポート

他の Docker プロジェクトと衝突しにくいよう、一般的なデフォルトポートからずらしています。

| Service | URL / Host port |
| --- | --- |
| React Web UI | `http://localhost:13000` |
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

これらの値は compose ファイルの `JG_*` 環境変数で変更できます。

## 環境設定

ローカル実行前に example ファイルをコピーします。

```powershell
Copy-Item .env.example .env
Copy-Item backend-dotnet/WebApi/appsettings.example.json backend-dotnet/WebApi/appsettings.json
Copy-Item frontend-react/.env.example frontend-react/.env
Copy-Item jerrygram-recommend/.env.example jerrygram-recommend/.env
Copy-Item backend-java/.env.example backend-java/.env
```

secret とポートの詳細は [docs/env-and-secrets.ja.md](docs/env-and-secrets.ja.md) を参照してください。

## ローカル起動

Docker インフラを起動します。

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka-elk-extended.yml up -d
```

必要に応じて .NET migration を適用します。

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

ブラウザで `http://localhost:13000` を開きます。

## Kafka と ELK の証跡

.NET API は `post-events`、`user-events`、`search-events`、`popular-searches` などの Kafka topic にイベントを発行します。イベントは Elasticsearch の `jerrygram-events-*` index に保存され、Kibana から確認できます。

![Kafka UI topics](docs/assets/screenshots/kafka-ui-topics.png)

![Kibana event indices](docs/assets/screenshots/kibana-indices.png)

詳しい流れは [docs/recommendation-and-kafka.ja.md](docs/recommendation-and-kafka.ja.md) にあります。

## ビルドとテスト

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

README 用スクリーンショット:

```powershell
cd frontend-react
npm run screenshots
```

GitHub Actions は .NET backend、Java backend、推薦サービス、React frontend の build/test のみを実行します。Pages 配信や本番デプロイは行いません。

## Notes

- 単一ノードのローカル Elasticsearch では replica が割り当てられないため、`yellow` が正常に表示されることがあります。
- seed 画像や過去の Blob URL が消えている場合、UI は fallback 画像を表示します。
- 他プロジェクトが `3000`、`6379`、`8080`、`9200` などを使う場合は、Jerrygram の `JG_*` ポートを維持するのがおすすめです。

## License

MIT License
