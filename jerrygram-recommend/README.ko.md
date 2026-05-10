# Jerrygram 추천 API

언어: [English](README.md) | 한국어 | [日本語](README.ja.md)

Jerrygram의 Node.js 추천 서비스입니다. PostgreSQL에서 최근 좋아요한 게시물 캡션을 읽고, OpenAI embedding을 생성한 뒤, cosine similarity로 후보 게시물을 점수화해 .NET API에 추천 결과를 반환합니다.

## 구조

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

## 명령어

```bash
npm install
npm start
npm run lint
npm audit --omit=dev
```

## Docker 메모

`.dockerignore`는 로컬 `node_modules`, `.env`, 로그, cache/build 산출물이 Docker build context에 들어가지 않도록 막습니다. 런타임 설정은 이미지에 복사하지 않고 Docker Compose 환경 변수로 전달합니다.
