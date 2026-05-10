# Jerrygram Seed Data

Language: [English](README.md) | [한국어](README.ko.md) | 日本語

このフォルダーには、feed ranking、Blob ベースの画像アップロード、Kafka search events、recommendation behavior を確認するための小さな demo dataset が入っています。

## Contents

| Path | Purpose |
| --- | --- |
| `seed-data.json` | users, posts, follows, likes, saves, repeated search terms |
| `images/kafka-trend.png` | Kafka/search trend demo 用 post image |
| `images/blob-storage.png` | local Blob Storage upload demo 用 post image |
| `images/recommendation-loop.png` | recommendation event demo 用 post image |
| `seed-jerrygram.ps1` | API based repeatable seed script |
| `verify-jerrygram-demo.ps1` | recommendation, Kafka, Elasticsearch demo evidence script |

## Suggested Demo Flow

1. `seed-data.json` の users を登録します。
2. 各 post image を caption と visibility に合わせて upload します。
3. follow relationships を作成します。
4. likes と saves を適用します。
5. search terms を順番に実行し、`SearchPerformed` events を十分に蓄積します。

現在の E2E tests も同じ scenario shape の mocked API responses を使います。そのため、full Docker stack がなくても CI で UI を検証できます。

## Seed Script

Docker stack と .NET API を先に起動してから実行します。

```powershell
.\infra\seed\seed-jerrygram.ps1
```

Windows execution policy で止まる場合は、現在の process にだけ bypass を付けて実行します。

```powershell
powershell -ExecutionPolicy Bypass -File .\infra\seed\seed-jerrygram.ps1
```

script はまず `seed-data.json` の username/email を使います。local DB に同じ account が別 password で存在する場合は、default の `_seed` suffix を付けた account、例えば `jerry_seed`、で自動 retry します。必要なら `-CollisionSuffix` で変更できます。

seed data を投入した後、live demo summary を確認します。

```powershell
.\infra\seed\verify-jerrygram-demo.ps1
```

```powershell
powershell -ExecutionPolicy Bypass -File .\infra\seed\verify-jerrygram-demo.ps1
```
