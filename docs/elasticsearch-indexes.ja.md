# Elasticsearch Index Inventory

Language: [English](elasticsearch-indexes.md) | [한국어](elasticsearch-indexes.ko.md) | 日本語

この文書は local Kibana/Elasticsearch UI に表示される indices を分類します。基準は 2026-05-10 に確認した local stack です。

## 維持対象

| Pattern / index | Reason |
| --- | --- |
| `.internal.alerts-*`, `.kibana*` | Kibana system indices と alerting internal indices です。通常の cleanup では削除しないでください。 |
| `posts` | 投稿検索と discovery に使う active app search index です。 |
| `users` | user search に使う active app search index です。 |
| `tags` | hashtag search に使う active app search index です。 |
| `jerrygram-events-post-*` | post analytics を示す Kafka/Logstash evidence index です。 |
| `jerrygram-events-user-*` | user analytics を示す Kafka/Logstash evidence index です。 |
| `jerrygram-events-search-*` | popular/trending search terms に使う Kafka/Logstash evidence index です。 |

## Cleanup Candidates

| Index | Observation | Recommendation |
| --- | --- | --- |
| `jerrygram-logs-000001` | documents `0`, alias `jerrygram-logs` | Logstash log alias demo が不要なら local cleanup candidate です。 |
| `jerrygram-dotnet-backend-2025.07.29` | old app log index, documents `1` | old log evidence が不要なら local archive/delete candidate です。 |
| `jerrygram-java-backend-2025.10.23` | old Java backend log index, documents `1` | Java log history が debugging や audit history に必要な場合のみ維持します。 |
| `jerrygram-java-backend-2026.05.09` | recent Java backend log index, documents `3` | Java backend demo には有用ですが、現在の React + .NET runtime path には必須ではありません。 |

## Yellow Status について

Local Elasticsearch は single-node setup です。replica を持つ index は別 node に replica shard を割り当てられないため `yellow` になることがあります。local development では通常の状態で、app が壊れているという意味ではありません。

## Useful Commands

App/event indices:

```powershell
Invoke-RestMethod "http://localhost:19200/_cat/indices?format=json&h=health,status,index,docs.count,store.size,creation.date.string"
```

Aliases:

```powershell
Invoke-RestMethod "http://localhost:19200/_cat/aliases?format=json&h=alias,index"
```

Local cleanup example. Run only when you intentionally want to remove local evidence data:

```powershell
Invoke-RestMethod -Method Delete "http://localhost:19200/jerrygram-logs-000001"
```

通常の demo 中は `.internal.*`, `.kibana*`, `posts`, `users`, `tags`, `jerrygram-events-*` を削除しないでください。
