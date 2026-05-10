# Jerrygram レコメンド API

Language: [English](README.md) | [한국어](README.ko.md) | 日本語

Jerrygram の Node.js レコメンドサービスです。PostgreSQL から最近いいねした投稿キャプションを読み、OpenAI embedding を生成し、cosine similarity で候補投稿をスコアリングして .NET API に並び替え済みの結果を返します。

## 構成

```text
jerrygram-recommend/
|- cache/
|  |- embeddingCache.js
|  |- hybridEmbeddingCache.js
|  \- redisEmbeddingCache.js
|- config/
|  |- app.js
|  |- database.js
|  |- openai.js
|  \- redis.js
|- controllers/
|  |- index.js
|  \- recommendController.js
|- middleware/
|  |- cors.js
|  |- errorHandler.js
|  |- logger.js
|  |- monitoring.js
|  \- security.js
|- models/
|  |- Post.js
|  |- RecommendationRequest.js
|  \- ValidationError.js
|- routes/
|  \- index.js
|- services/
|  |- embeddingService.js
|  |- postRepository.js
|  \- recommendService.js
|- test/
|  \- recommendation.test.js
|- utils/
|  \- cosine.js
|- validation/
|  \- validators.js
|- .dockerignore
|- .env.example
|- Dockerfile
|- index.js
|- package-lock.json
\- package.json
```

## エンドポイント

```http
GET /recommend?userId={userId}&limit=10
GET /health
```

## 環境変数

ローカル開発では `.env.example` を `.env` にコピーします。

```bash
DATABASE_URL=postgresql://postgres:test@localhost:15433/jerrygram
OPENAI_API_KEY=your_openai_api_key_here
PORT=3001
NODE_ENV=development
CORS_ORIGINS=http://localhost:13000
MAX_CANDIDATE_POSTS=100
MAX_RECOMMENDATIONS=10
MAX_USER_CAPTIONS=10
ENABLE_CACHE=true
CACHE_EXPIRY=3600
USE_REDIS_CACHE=true
REDIS_URL=redis://localhost:16380
REDIS_PASSWORD=
```

## ローカルコマンド

```bash
npm install
npm start
npm test
npm run lint
```

## Docker メモ

`.dockerignore` はローカル `node_modules`, `.env`, ログ, キャッシュ/ビルド出力を Docker build context から除外します。ランタイム設定はイメージにコピーせず、Docker Compose の環境変数で渡します。
