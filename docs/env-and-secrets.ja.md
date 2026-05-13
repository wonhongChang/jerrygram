# 環境変数とシークレット設定

Language: [English](env-and-secrets.md) | [한국어](env-and-secrets.ko.md) | 日本語

Jerrygram はローカルシークレットを Git に含めません。`*.example` ファイルだけをコミットし、環境構築時にローカル実行用ファイルへコピーして使います。

## ローカルファイル

| 目的 | サンプルファイル | ローカルファイル |
| --- | --- | --- |
| Docker ポートと共通シークレット | `.env.example` | `.env` |
| .NET API 設定 | `backend-dotnet/WebApi/appsettings.example.json` | `backend-dotnet/WebApi/appsettings.json` |
| React 開発サーバー | `frontend-react/.env.example` | `frontend-react/.env` |
| Node レコメンドサービス | `jerrygram-recommend/.env.example` | `jerrygram-recommend/.env` |
| Java バックエンド | `backend-java/.env.example` | `backend-java/.env` |

## 現在のローカルポート

| サービス | ホストポート |
| --- | ---: |
| React web | `13000` |
| .NET API | `5018` |
| レコメンドサービス | `13001` |
| PostgreSQL | `15433` |
| Redis | `16380` |
| Elasticsearch | `19200` |
| Kafka broker | `19092` |
| Kafka UI | `18081` |
| Kafka Connect | `18083` |
| Kibana | `15601` |
| Logstash API | `19600` |

## Stream Processing 設定

`SearchTrendStreamProcessor` は Kafka `search-events` を consume し、Redis search trend bucket を更新します。ローカルで restart しても Kafka offset を再利用できるよう、consumer group は安定した値にします。

```json
"SearchTrendStreamProcessor": {
  "Enabled": true,
  "BootstrapServers": "localhost:19092",
  "ConsumerGroup": "jerrygram-search-trend-processor"
}
```

## GitHub Actions

CI workflow は production secret なしで build/test します。`OPENAI_API_KEY`、Azure storage credentials、deployment credentials などの runtime-only secret は、将来 deploy workflow を導入するときに GitHub repository secrets として追加します。

## メモ

- `JwtSettings:SecretKey`、database password、Redis password、cloud storage connection string はコミット済み設定に含めません。
- Docker ポートは他のローカルプロジェクトとの衝突を減らすため、一般的な既定値から意図的にずらしています。
- Local Blob Storage は設定された provider を使用します。`UseDevelopmentStorage=true` は Azurite 互換 local storage を対象にし、実 Azure connection string もコード変更なしで利用できます。
