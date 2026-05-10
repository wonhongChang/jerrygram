# Jerrygram Seed Data

Language: English | [한국어](README.ko.md) | [日本語](README.ja.md)

This folder contains a small data set for demonstrating feed ranking, blob-backed images, Kafka search events, and recommendation behavior.

## Contents

| Path | Purpose |
| --- | --- |
| `seed-data.json` | Users, posts, follows, likes, saves, and repeated search terms |
| `images/kafka-trend.png` | Post image for Kafka/search trend demos |
| `images/blob-storage.png` | Post image for local blob storage upload demos |
| `images/recommendation-loop.png` | Post image for recommendation event demos |
| `seed-jerrygram.ps1` | Idempotent API seed script |
| `verify-jerrygram-demo.ps1` | Demo evidence script for recommendation, Kafka, and Elasticsearch |

## Suggested Demo Flow

1. Register the users in `seed-data.json`.
2. Upload each post image with the matching caption and visibility.
3. Create the follow relationships.
4. Apply likes and saves.
5. Run the search terms in order to publish enough search events for popular/trending search screens.

The current E2E tests use the same scenario shape with mocked API responses, so the UI can be validated in CI without requiring the full Docker stack.

## Run The Seed Script

Start the Docker stack and .NET API first, then run:

```powershell
.\infra\seed\seed-jerrygram.ps1
```

If Windows blocks script execution, run it with a process-local bypass:

```powershell
powershell -ExecutionPolicy Bypass -File .\infra\seed\seed-jerrygram.ps1
```

The script first tries the usernames and emails from `seed-data.json`. If a local database already has those accounts with different credentials, it automatically retries with the default `_seed` suffix, for example `jerry_seed`. You can override that with `-CollisionSuffix`.

After seed data is loaded, capture a live demo summary:

```powershell
.\infra\seed\verify-jerrygram-demo.ps1
```

```powershell
powershell -ExecutionPolicy Bypass -File .\infra\seed\verify-jerrygram-demo.ps1
```
