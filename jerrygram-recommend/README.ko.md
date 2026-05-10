# Jerrygram 추천 API

언어: [English](README.md) | 한국어 | [日本語](README.ja.md)

Jerrygram의 Node.js 추천 서비스입니다. PostgreSQL에서 최근 좋아요한 게시물 캡션을 읽고, OpenAI embedding을 생성한 뒤 cosine similarity로 후보 게시물을 점수화하여 .NET API에 정렬된 추천 결과를 반환합니다.

## 구조

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

## 엔드포인트

```http
GET /recommend?userId={userId}&limit=10
GET /health
```

## 환경 변수

로컬 개발에서는 `.env.example`을 `.env`로 복사합니다.

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

## 로컬 명령어

```bash
npm install
npm start
npm test
npm run lint
```

## Docker 참고

`.dockerignore`는 로컬 `node_modules`, `.env`, 로그, 캐시/빌드 결과물을 Docker build context에서 제외합니다. 런타임 설정은 이미지에 복사하지 않고 Docker Compose 환경 변수로 전달합니다.
