# Jerrygram

언어: [English](README.md) | 한국어 | [日本語](README.ja.md)

Jerrygram은 React, ASP.NET Core, PostgreSQL, Redis, Azure Blob Storage 연동, Elasticsearch, Kafka, Logstash, Kibana, Node.js 추천 서비스를 함께 사용하는 Instagram 스타일 풀스택 소셜 앱입니다. 로컬에서도 운영에 가까운 구조를 확인할 수 있도록 구성되어 있습니다.

현재 검증된 로컬 개발 경로는 다음과 같습니다.

- React 웹 UI: `http://localhost:13000`
- ASP.NET Core Web API: `http://localhost:5018`
- PostgreSQL, Redis, Elasticsearch, Kafka, Kafka UI, Logstash, Kibana, Kafka Connect, 추천 서비스는 Docker로 실행

이 저장소에는 Java/Spring 백엔드도 대체 구현으로 포함되어 있지만, 현재 웹 UI는 기본적으로 .NET API에 연결됩니다.

## 아키텍처 개요

![Jerrygram 아키텍처](docs/assets/jerrygram-architecture.png)

## 현재 기능

- JWT 기반 회원가입, 로그인, 로그아웃, 현재 사용자 로딩
- multipart 업로드를 통한 사진 게시물 생성
- 홈 피드, 공개 게시물, 게시물 상세, 탐색, 프로필, 검색 페이지
- 좋아요, 댓글, 팔로우, 알림, 프로필 편집
- 저장/북마크 게시물과 프로필 `Saved` 탭
- Redis 기반 캐싱과 인메모리 fallback
- Elasticsearch 기반 검색과 탐색
- .NET API의 Kafka 이벤트 발행
- Kafka에서 Logstash를 거쳐 Elasticsearch로 이어지는 이벤트 분석 파이프라인
- `jerrygram-events-*`용 Kibana data view 지원
- 별도 Node.js 추천 서비스

## 로컬 포트

Jerrygram은 다른 Docker 프로젝트와 충돌하지 않도록 기본 포트가 아닌 별도 호스트 포트를 사용합니다.

| 서비스 | URL / 호스트 포트 |
| --- | --- |
| React 웹 UI | `http://localhost:13000` |
| ASP.NET Core API | `http://localhost:5018` |
| 추천 서비스 | `http://localhost:13001` |
| PostgreSQL | `localhost:15433` |
| Redis | `localhost:16380` |
| Elasticsearch | `http://localhost:19200` |
| Elasticsearch transport | `localhost:19300` |
| Kafka | `localhost:19092` |
| Kafka JMX | `localhost:19997` |
| Kafka UI | `http://localhost:18081` |
| Kibana | `http://localhost:15601` |
| Logstash Beats | `localhost:15044` |
| Logstash API | `http://localhost:19600` |
| Kafka Connect | `http://localhost:18083` |

이 기본값들은 compose 파일의 `JG_*` 환경 변수로 변경할 수 있습니다.

## 프로젝트 구조

```text
jerrygram/
  backend-dotnet/                 ASP.NET Core API
    Domain/                       도메인 엔티티와 enum
    Application/                  CQRS command, query, handler, DTO
    Infrastructure/               Redis, Kafka, Elasticsearch, Blob, JWT
    Persistence/                  EF Core DbContext, repository, migration
    WebApi/                       controller, middleware, app configuration
  backend-java/                   대체 Spring Boot API 구현
  frontend-react/                 React + TypeScript 웹 앱
  jerrygram-recommend/            Node.js 추천 서비스
  infra/                          Elasticsearch, Kafka, Kibana 설정 스크립트
  logstash/                       Logstash pipeline 설정
  docker-compose.yml              핵심 인프라와 추천 서비스
  docker-compose.kafka-elk-extended.yml
                                  Elasticsearch, Kafka, Kibana, Logstash 스택
```

## 필요 조건

- Docker Desktop
- .NET SDK 8 이상
- Node.js와 npm
- Windows PowerShell

선택 사항:

- 수동 migration 작업을 위한 `dotnet-ef`
- fallback 이미지가 아닌 실제 이미지 저장을 사용하려면 Azure Storage 또는 Azurite 호환 Blob 설정

## 환경 설정

저장소 루트에 `.env` 파일을 만들거나 수정합니다. 로컬 compose 스택에는 최소한 Redis와 추천 서비스 관련 값이 필요합니다.

```env
REDIS_PASSWORD=your-local-redis-password
OPENAI_API_KEY=your-openai-api-key
JG_RECOMMEND_PORT=13001
JG_POSTGRES_PORT=15433
JG_REDIS_PORT=16380
JG_ELASTICSEARCH_PORT=19200
JG_KAFKA_PORT=19092
JG_KAFKA_UI_PORT=18081
JG_KIBANA_PORT=15601
JG_KAFKA_CONNECT_PORT=18083
```

React 앱은 .NET API를 바라보도록 설정합니다.

```env
# frontend-react/.env
REACT_APP_API_URL=http://localhost:5018/api
PORT=13000
```

## 로컬 스택 실행

Docker 인프라를 실행합니다.

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka-elk-extended.yml up -d
```

필요한 경우 .NET 데이터베이스 migration을 적용합니다.

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

앱을 엽니다.

```text
http://localhost:13000
```

## 상태 확인

자주 쓰는 로컬 점검 명령입니다.

```powershell
docker ps
Invoke-WebRequest http://localhost:13000 -UseBasicParsing
Invoke-WebRequest http://localhost:5018/api/posts?page=1&pageSize=3 -UseBasicParsing
Invoke-WebRequest http://localhost:13001/health -UseBasicParsing
Invoke-RestMethod http://localhost:19200/_cluster/health
Invoke-WebRequest http://localhost:15601/api/status -UseBasicParsing
Invoke-WebRequest http://localhost:19600/_node/stats -UseBasicParsing
```

Kafka 토픽 확인:

```powershell
docker exec jg-kafka kafka-topics --bootstrap-server kafka:29092 --list
```

Jerrygram 이벤트 토픽은 다음을 기대합니다.

```text
post-events
user-events
search-events
popular-searches
```

Elasticsearch 이벤트 인덱스 확인:

```powershell
Invoke-RestMethod "http://localhost:19200/_cat/indices/jerrygram-events-*?format=json&h=index,docs.count,health,status"
```

## 검증된 UI 흐름

현재 로컬 UI는 .NET API 기준으로 다음 흐름을 검증했습니다.

1. `/register`에서 새 사용자 가입
2. 이미지와 캡션으로 게시물 생성
3. 홈 피드에 게시물이 표시되는지 확인
4. 피드에서 게시물 저장
5. 프로필 `Saved` 탭에 표시되는지 확인
6. 상세 페이지에서 저장 상태 확인
7. 댓글 액션 클릭 시 댓글 입력창에 포커스가 이동하는지 확인
8. 생성한 사용자가 검색되는지 확인
9. 피드 옵션 메뉴에서 게시물 삭제
10. 삭제된 게시물이 API에서 `404`를 반환하는지 확인

## Kafka와 ELK

.NET API는 `post-events`, `user-events`, `search-events`, `popular-searches` 같은 Kafka 토픽으로 이벤트를 발행합니다.

Logstash는 Kafka 이벤트를 소비해 일별 Elasticsearch 인덱스에 기록합니다.

```text
jerrygram-events-post-YYYY.MM.DD
jerrygram-events-user-YYYY.MM.DD
jerrygram-events-search-YYYY.MM.DD
```

Kibana는 다음 주소에서 사용할 수 있습니다.

```text
http://localhost:15601
```

`jerrygram-events-*`에는 `Jerrygram Events` data view를 사용합니다.

## 개발 참고 사항

- 단일 노드 로컬 환경에서는 replica가 배치되지 않아 Elasticsearch 상태가 `yellow`로 보일 수 있습니다. 로컬 개발에서는 정상 범주입니다.
- 기존 seed 이미지 URL은 원격 Blob이 사라진 경우 `404`를 반환할 수 있습니다. UI는 이런 경우 fallback 이미지를 렌더링합니다.
- 게시물 생성, 수정, 삭제, 좋아요, 좋아요 취소, 저장, 저장 취소 후에는 피드, 공개 게시물, 저장 게시물, 상세 페이지, 탐색 데이터 캐시를 넓게 무효화합니다.
- 홈 피드는 팔로우한 사용자의 게시물뿐 아니라 현재 사용자의 게시물도 포함합니다.
- 다른 프로젝트가 `3000`, `6379`, `8080`, `9200` 같은 흔한 포트를 사용 중이면 위의 `JG_*` 포트를 유지하는 편이 좋습니다.

## 빌드 명령

백엔드:

```powershell
dotnet build backend-dotnet/WebApi/WebApi.csproj
```

프론트엔드:

```powershell
cd frontend-react
npm run build
```

## 라이선스

이 프로젝트는 MIT 라이선스를 따릅니다.
