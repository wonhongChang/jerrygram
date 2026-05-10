# Jerrygram Recommend API

Language: [English](README.md) | [한국어](README.ko.md) | 日本語

Jerrygram の Node.js recommendation service です。PostgreSQL から最近 liked した post captions を読み、OpenAI embeddings を生成し、cosine similarity で candidate posts を score して .NET API に recommendation results を返します。

## Structure

```text
jerrygram-recommend/
├── cache/
│   ├── embeddingCache.js
│   ├── hybridEmbeddingCache.js
│   └── redisEmbeddingCache.js
├── config/
│   ├── app.js
│   ├── database.js
│   ├── openai.js
│   └── redis.js
├── controllers/
│   ├── index.js
│   └── recommendController.js
├── middleware/
├── models/
├── routes/
├── services/
├── utils/
├── validation/
├── .dockerignore
├── Dockerfile
└── index.js
```

## Endpoints

```http
GET /recommend?userId={userId}&limit=10
GET /health
```

## Environment

Local development では `.env.example` を `.env` にコピーします。

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

## Commands

```bash
npm install
npm start
npm run lint
npm audit --omit=dev
```

## Docker Notes

`.dockerignore` excludes local `node_modules`, `.env`, logs, and cache/build outputs from the Docker build context. Runtime settings should be passed through Docker Compose environment variables rather than copied into the image.
