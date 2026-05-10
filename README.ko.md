# Jerrygram

언어: [English](README.md) | 한국어 | [日本語](README.ja.md)

Jerrygram은 Instagram 스타일의 풀스택 소셜 앱입니다. React, ASP.NET Core, PostgreSQL, Redis, 로컬/Azure 호환 Blob Storage, Elasticsearch, Kafka, Logstash, Kibana, Node.js 추천 서비스를 한 로컬 스택에서 재현할 수 있게 구성했습니다.

현재 검증된 기본 실행 경로는 다음과 같습니다.

- React 웹 UI: `http://localhost:13000`
- ASP.NET Core Web API: `http://localhost:5018`
- PostgreSQL, Redis, Elasticsearch, Kafka, Kafka UI, Logstash, Kibana, Kafka Connect, 추천 서비스는 Docker로 실행

저장소에는 Java/Spring 백엔드도 대체 구현으로 포함되어 있지만, 현재 웹 UI는 기본적으로 .NET API에 연결됩니다.

## 아키텍처

![Jerrygram architecture](docs/assets/jerrygram-architecture.png)

백엔드 내부 구조는 [docs/backend-architecture.md](docs/backend-architecture.md)에 정리되어 있습니다.

## 스크린샷

![회원가입 화면](docs/assets/screenshots/jerrygram-register.png)

![피드 화면](docs/assets/screenshots/jerrygram-feed.png)

![검색 트렌드 화면](docs/assets/screenshots/jerrygram-search.png)

## 문서와 증거 자료

- [백엔드 아키텍처](docs/backend-architecture.ko.md)
- [추천과 Kafka 증거](docs/recommendation-and-kafka.ko.md)
- [Elasticsearch 인덱스 목록과 정리 기준](docs/elasticsearch-indexes.ko.md)
- [환경 변수와 secret 정리](docs/env-and-secrets.ko.md)
- [Seed 데이터](infra/seed/README.ko.md)

## 주요 기능

- JWT 기반 회원가입, 로그인, 로그아웃, 현재 사용자 로딩
- multipart 이미지 업로드 기반 게시물 생성
- 홈 피드, 공개 게시물, 게시물 상세, 탐색, 프로필, 검색 화면
- 좋아요, 댓글, 팔로우, 알림, 프로필 수정, 저장한 게시물
- Redis 캐시와 인메모리 fallback
- Elasticsearch 기반 검색과 탐색
- .NET API에서 Kafka 이벤트 발행
- Kafka에서 Logstash/Kafka Connect를 거쳐 Elasticsearch로 이어지는 분석 파이프라인
- `jerrygram-events-*` Kibana 확인 지원
- 캡션 임베딩과 cosine similarity 기반 Node.js 추천 서비스
- 회원가입, 피드 상호작용, Kafka 기반 검색 트렌드를 검증하는 Playwright E2E 테스트

## 로컬 포트

다른 Docker 프로젝트와 충돌하지 않도록 기본 포트를 일반적인 기본값에서 옮겨두었습니다.

| 서비스 | URL / 호스트 포트 |
| --- | --- |
| React 웹 UI | `http://localhost:13000` |
| ASP.NET Core API | `http://localhost:5018` |
| 추천 서비스 | `http://localhost:13001` |
| PostgreSQL | `localhost:15433` |
| Redis | `localhost:16380` |
| Elasticsearch | `http://localhost:19200` |
| Kafka | `localhost:19092` |
| Kafka UI | `http://localhost:18081` |
| Kibana | `http://localhost:15601` |
| Logstash API | `http://localhost:19600` |
| Kafka Connect | `http://localhost:18083` |

이 값들은 compose 파일의 `JG_*` 환경 변수로 변경할 수 있습니다.

## 실행 준비

```powershell
Copy-Item .env.example .env
Copy-Item backend-dotnet/WebApi/appsettings.example.json backend-dotnet/WebApi/appsettings.json
Copy-Item frontend-react/.env.example frontend-react/.env
Copy-Item jerrygram-recommend/.env.example jerrygram-recommend/.env
Copy-Item backend-java/.env.example backend-java/.env
```

자세한 secret과 포트 설명은 [docs/env-and-secrets.ko.md](docs/env-and-secrets.ko.md)를 참고하세요.

## 로컬 실행

Docker 인프라를 실행합니다.

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka-elk-extended.yml up -d
```

필요하면 .NET 마이그레이션을 적용합니다.

```powershell
dotnet ef database update `
  --project backend-dotnet/Persistence/Persistence.csproj `
  --startup-project backend-dotnet/WebApi/WebApi.csproj
```

.NET API를 실행합니다.

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project backend-dotnet/WebApi/WebApi.csproj --urls http://localhost:5018
```

React 앱을 실행합니다.

```powershell
cd frontend-react
npm install
npm start
```

브라우저에서 `http://localhost:13000`을 엽니다.

## Kafka와 ELK 증거

.NET API는 `post-events`, `user-events`, `search-events`, `popular-searches` 같은 Kafka topic으로 이벤트를 발행합니다. 이벤트는 Elasticsearch의 `jerrygram-events-*` 인덱스로 저장되고 Kibana에서 확인할 수 있습니다.

![Kafka UI topics](docs/assets/screenshots/kafka-ui-topics.png)

![Kibana event indices](docs/assets/screenshots/kibana-indices.png)

자세한 흐름은 [docs/recommendation-and-kafka.ko.md](docs/recommendation-and-kafka.ko.md)에 있습니다.

## 빌드와 테스트

.NET 백엔드:

```powershell
dotnet build backend-dotnet/WebApi/WebApi.csproj
```

React 프론트엔드:

```powershell
cd frontend-react
npm run test:ci
npm run build
npm run e2e
```

README용 스크린샷 생성:

```powershell
cd frontend-react
npm run screenshots
```

GitHub Actions는 .NET 백엔드, Java 백엔드, 추천 서비스, React 프론트엔드의 build/test만 실행합니다. Pages 배포나 서비스 배포는 하지 않습니다.

## 참고

- 단일 노드 로컬 Elasticsearch는 replica가 배정되지 않아 `yellow` 상태가 정상적으로 보일 수 있습니다.
- seed 이미지나 과거 Blob URL이 사라진 경우 UI는 fallback 이미지를 표시합니다.
- 다른 프로젝트가 `3000`, `6379`, `8080`, `9200` 같은 포트를 쓰고 있다면 Jerrygram은 현재 `JG_*` 포트를 유지하는 편이 좋습니다.

## 라이선스

MIT License
