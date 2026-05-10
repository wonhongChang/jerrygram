# Jerrygram Recommend API

Language: English | [한국어](README.ko.md) | [日本語](README.ja.md)

Node.js recommendation service for Jerrygram. It reads recent liked post captions from PostgreSQL, generates embeddings with OpenAI, scores candidate posts by cosine similarity, and returns ranked recommendations to the .NET API.

## Structure

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

## Endpoints

```http
GET /recommend?userId={userId}&limit=10
GET /health
```

## Environment

Copy `.env.example` to `.env` for local development.

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

## Local Commands

```bash
npm install
npm start
npm test
npm run lint
```

## Docker Notes

`.dockerignore` excludes local `node_modules`, `.env`, logs, and cache/build outputs from the Docker build context. Runtime settings should be passed through Docker Compose environment variables rather than copied into the image.
