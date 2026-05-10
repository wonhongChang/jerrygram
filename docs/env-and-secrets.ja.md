# 環境変数と Secret 設定

Language: [English](env-and-secrets.md) | [한국어](env-and-secrets.ko.md) | 日本語

Jerrygram は local secret を Git に含めません。`*.example` files だけを commit し、各開発環境では local runtime file にコピーして利用します。

## ローカルファイル

| Purpose | Example file | Local file |
| --- | --- | --- |
| Docker ports and shared secrets | `.env.example` | `.env` |
| .NET API settings | `backend-dotnet/WebApi/appsettings.example.json` | `backend-dotnet/WebApi/appsettings.json` |
| React dev server | `frontend-react/.env.example` | `frontend-react/.env` |
| Node recommendation service | `jerrygram-recommend/.env.example` | `jerrygram-recommend/.env` |
| Java backend | `backend-java/.env.example` | `backend-java/.env` |

## 現在のローカルポート

| Service | Host port |
| --- | ---: |
| React web | `13000` |
| .NET API | `5018` |
| Recommendation service | `13001` |
| PostgreSQL | `15433` |
| Redis | `16380` |
| Elasticsearch | `19200` |
| Kafka broker | `19092` |
| Kafka UI | `18081` |
| Kafka Connect | `18083` |
| Kibana | `15601` |
| Logstash API | `19600` |

## GitHub Actions

現在の CI workflow は production secret なしで build/test だけを実行します。`OPENAI_API_KEY`、Azure storage credentials、deployment credentials などの runtime secret は、deploy workflow を追加するときに GitHub repository secrets として設定します。

## Notes

- `JwtSettings:SecretKey`、database password、Redis password、cloud storage connection string は commit しません。
- Docker ports は他の local project との衝突を避けるため、一般的な default からずらしています。
- Local Blob Storage は設定された provider を使います。`UseDevelopmentStorage=true` は Azurite 互換の local storage を指し、実 Azure connection string も code 変更なしで利用できます。
