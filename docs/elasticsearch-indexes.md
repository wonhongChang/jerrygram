# Elasticsearch Index Inventory

Language: English | [한국어](elasticsearch-indexes.ko.md) | [日本語](elasticsearch-indexes.ja.md)

This document classifies the indices visible in the local Kibana/Elasticsearch UI. It is based on the local stack checked on 2026-05-10.

## Keep

| Pattern / index | Why |
| --- | --- |
| `.internal.alerts-*`, `.kibana*` | Kibana system indices and alerting internals. Do not delete during normal cleanup. |
| `posts` | Active app search index for post search and discovery. Current local count: `16`. |
| `users` | Active app search index for user search. Current local count: `18`. |
| `tags` | Active app search index for hashtag search. Current local count: `9`. |
| `jerrygram-events-post-*` | Kafka/Logstash event evidence for post analytics. Current local counts include `12` on 2026-05-09 and `5` on 2026-05-10. |
| `jerrygram-events-user-*` | Kafka/Logstash event evidence for user analytics. Current local counts include `45` on 2026-05-09 and `12` on 2026-05-10. |
| `jerrygram-events-search-*` | Kafka/Logstash event evidence for popular and trending search terms. Current local counts include `8` on 2026-05-09 and `8` on 2026-05-10. |

## Cleanup Candidates

| Index | Current observation | Recommendation |
| --- | --- | --- |
| `jerrygram-logs-000001` | `0` documents, alias `jerrygram-logs` | Safe local cleanup candidate if the Logstash log alias is not being demonstrated. |
| `jerrygram-dotnet-backend-2025.07.29` | Old app log index with `1` document | Keep only if old .NET log evidence is useful; otherwise archive/delete locally. |
| `jerrygram-java-backend-2025.10.23` | Old Java backend log index with `1` document | Keep only if old Java log evidence is useful. |
| `jerrygram-java-backend-2026.05.09` | Recent Java backend log index with `3` documents | Optional. Useful when demonstrating the Java backend, not required by the active React + .NET runtime path. |

## Why Some Indices Are Yellow

The local Elasticsearch stack is a single-node setup. Indices with replicas can show `yellow` because replica shards cannot be assigned to another node. This is expected locally and does not mean the app is broken.

## Useful Commands

Inspect app and event indices:

```powershell
Invoke-RestMethod "http://localhost:19200/_cat/indices?format=json&h=health,status,index,docs.count,store.size,creation.date.string"
```

Inspect aliases:

```powershell
Invoke-RestMethod "http://localhost:19200/_cat/aliases?format=json&h=alias,index"
```

Local cleanup example. Run only when you intentionally want to remove local evidence data:

```powershell
Invoke-RestMethod -Method Delete "http://localhost:19200/jerrygram-logs-000001"
```

Do not delete `.internal.*`, `.kibana*`, `posts`, `users`, `tags`, or `jerrygram-events-*` during normal project demos.
