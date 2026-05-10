# Jerrygram シードデータ

Language: [English](README.md) | [한국어](README.ko.md) | 日本語

このフォルダには、フィードランキング、Blob Storage 画像、Kafka 検索イベント、レコメンド動作を示すための小さなデータセットが含まれています。

## 内容

| パス | 目的 |
| --- | --- |
| `seed-data.json` | ユーザー、投稿、フォロー、いいね、保存、繰り返し検索語 |
| `images/kafka-trend.png` | Kafka/検索トレンドデモ用の投稿画像 |
| `images/blob-storage.png` | ローカル Blob Storage アップロードデモ用の投稿画像 |
| `images/recommendation-loop.png` | レコメンドイベントデモ用の投稿画像 |
| `seed-jerrygram.ps1` | 冪等に実行できる API シードスクリプト |
| `verify-jerrygram-demo.ps1` | レコメンド、Kafka、Elasticsearch のデモ証跡スクリプト |

## 推奨デモフロー

1. `seed-data.json` のユーザーを登録します。
2. 各投稿画像を対応するキャプションと公開範囲でアップロードします。
3. フォロー関係を作成します。
4. いいねと保存を適用します。
5. 人気/トレンド検索画面に十分なイベントが入るよう、検索語を順番に実行します。

現在の E2E テストは同じシナリオを mock API 応答で検証するため、CI ではフル Docker スタックなしで UI を確認できます。

## シードスクリプト実行

先に Docker スタックと .NET API を起動してから実行します。

```powershell
.\infra\seed\seed-jerrygram.ps1
```

Windows でスクリプト実行がブロックされる場合は、プロセス単位の bypass で実行します。

```powershell
powershell -ExecutionPolicy Bypass -File .\infra\seed\seed-jerrygram.ps1
```

スクリプトはまず `seed-data.json` のユーザー名とメールを使います。ローカル DB に同じアカウントが別の認証情報で存在する場合、既定の `_seed` suffix を付けて再試行します。例: `jerry_seed`。`-CollisionSuffix` で変更できます。

シードデータ投入後、ライブデモの概要を取得します。

```powershell
.\infra\seed\verify-jerrygram-demo.ps1
```

```powershell
powershell -ExecutionPolicy Bypass -File .\infra\seed\verify-jerrygram-demo.ps1
```
