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

## Suggested Demo Flow

1. Register the users in `seed-data.json`.
2. Upload each post image with the matching caption and visibility.
3. Create the follow relationships.
4. Apply likes and saves.
5. Run the search terms in order to publish enough `SearchPerformed` events for popular/trending search screens.

The current E2E tests use the same scenario shape with mocked API responses, so the UI can be validated in CI without requiring the full Docker stack.
