# Jerrygram Seed Data

Language: [English](README.md) | [한국어](README.ko.md) | 日本語

この folder は feed ranking、Blob based image upload、Kafka search events、recommendation behavior を示すための小さな demo dataset です。

## Contents

| Path | Purpose |
| --- | --- |
| `seed-data.json` | users, posts, follows, likes, saves, repeated search terms |
| `images/kafka-trend.png` | Kafka/search trend demo 用 post image |
| `images/blob-storage.png` | local Blob Storage upload demo 用 post image |
| `images/recommendation-loop.png` | recommendation event demo 用 post image |

## Suggested Demo Flow

1. `seed-data.json` の users を登録します。
2. 各 post image に対応する caption と visibility で upload します。
3. follow relationships を作成します。
4. likes と saves を適用します。
5. search terms を順番に実行し、`SearchPerformed` events を十分に蓄積します。

現在の E2E tests も同じ scenario shape の mocked API responses を使います。そのため、full Docker stack がなくても CI で UI を検証できます。
