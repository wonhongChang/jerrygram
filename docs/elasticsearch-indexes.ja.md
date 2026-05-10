# Elasticsearch インデックス一覧

Language: [English](elasticsearch-indexes.md) | [한국어](elasticsearch-indexes.ko.md) | 日本語

この文書は、ローカル Kibana/Elasticsearch UI で見えるインデックスを分類します。基準は 2026-05-10 に確認したローカルスタックです。

## 維持対象

| パターン / インデックス | 理由 |
| --- | --- |
| `.internal.alerts-*`, `.kibana*` | Kibana system index と alerting internal index です。通常の整理では削除しません。 |
| `posts` | 投稿検索と探索に使う active app search index です。現在のローカル件数: `16`. |
| `users` | ユーザー検索に使う active app search index です。現在のローカル件数: `18`. |
| `tags` | ハッシュタグ検索に使う active app search index です。現在のローカル件数: `9`. |
| `jerrygram-events-post-*` | 投稿 analytics の Kafka/Logstash event evidence です。現在のローカル件数は 2026-05-09 `12`、2026-05-10 `5` です。 |
| `jerrygram-events-user-*` | ユーザー analytics の Kafka/Logstash event evidence です。現在のローカル件数は 2026-05-09 `45`、2026-05-10 `12` です。 |
| `jerrygram-events-search-*` | 人気/トレンド検索語の Kafka/Logstash event evidence です。現在のローカル件数は 2026-05-09 `8`、2026-05-10 `8` です。 |

## 整理候補

| インデックス | 現在の観察 | 推奨 |
| --- | --- | --- |
| `jerrygram-logs-000001` | `0` documents、alias `jerrygram-logs` | Logstash log alias をデモしない場合、ローカル整理候補です。 |
| `jerrygram-dotnet-backend-2025.07.29` | 古い .NET app log index、`1` document | 過去ログ証跡が必要な場合のみ維持し、それ以外は archive/delete できます。 |
| `jerrygram-java-backend-2025.10.23` | 古い Java backend log index、`1` document | 古い Java log evidence が必要な場合のみ維持します。 |
| `jerrygram-java-backend-2026.05.09` | 最近の Java backend log index、`3` documents | 任意です。Java backend を示すときは有用ですが、現在の React + .NET 実行経路には必須ではありません。 |

## 一部インデックスが yellow になる理由

ローカル Elasticsearch は single-node 構成です。replica shard を別ノードに割り当てられないため、replica を持つインデックスは `yellow` になることがあります。ローカルでは想定内で、アプリが壊れているという意味ではありません。

## 便利なコマンド

アプリ/イベントインデックス確認:

```powershell
Invoke-RestMethod "http://localhost:19200/_cat/indices?format=json&h=health,status,index,docs.count,store.size,creation.date.string"
```

alias 確認:

```powershell
Invoke-RestMethod "http://localhost:19200/_cat/aliases?format=json&h=alias,index"
```

ローカル整理例です。ローカル証跡データを意図して削除する場合のみ実行してください。

```powershell
Invoke-RestMethod -Method Delete "http://localhost:19200/jerrygram-logs-000001"
```

通常のデモ中は `.internal.*`, `.kibana*`, `posts`, `users`, `tags`, `jerrygram-events-*` を削除しないでください。
