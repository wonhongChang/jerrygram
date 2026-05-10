# Jerrygram 백엔드 아키텍처

언어: [English](backend-architecture.md) | 한국어 | [日本語](backend-architecture.ja.md)

이 문서는 현재 웹 UI가 사용하는 `backend-dotnet` 기준의 백엔드 구조를 설명합니다. Java/Spring 백엔드는 대체 구현으로 남아 있지만, 현재 검증된 실행 경로는 ASP.NET Core Web API입니다.

![Jerrygram backend architecture](assets/backend-architecture.svg)

## 요약

요청은 `WebApi`에서 시작해 `Application`, `Domain`으로 흐릅니다. 저장소와 외부 시스템은 `Persistence`, `Infrastructure`가 담당합니다. 게시물 생성, 수정, 삭제 같은 쓰기 작업은 PostgreSQL, Blob Storage, Elasticsearch, Redis cache, Kafka 이벤트 파이프라인을 함께 갱신합니다.

## 레이어 책임

| 레이어 | 책임 | 주요 파일 |
| --- | --- | --- |
| `WebApi` | HTTP 진입점, 인증, 검증, 미들웨어, DI | `Controllers`, `Middleware`, `ServiceExtensions.cs` |
| `Application` | CQRS 핸들러, DTO, 서비스 인터페이스, 이벤트 | `Commands`, `Queries`, `Interfaces`, `Events` |
| `Domain` | 핵심 엔티티, enum, 값 객체 | `User`, `Post`, `Comment`, `PostCaption`, `PostVisibility` |
| `Persistence` | EF Core DbContext, repository, migration | `AppDbContext`, `Repositories` |
| `Infrastructure` | Redis, Kafka, Elasticsearch, Blob, JWT, 추천 클라이언트 | `Services` |

## 주요 흐름

### 게시물 생성/수정

1. `PostController`가 multipart/form-data 요청을 받습니다.
2. Web API request DTO가 `IFormFile`을 `Application.Common.UploadFile`로 변환합니다.
3. Command handler가 도메인 모델을 갱신합니다.
4. `BlobService`가 이미지를 저장하고 공개 이미지 URL을 반환합니다.
5. EF Core repository가 PostgreSQL에 저장합니다.
6. `ElasticService`가 검색용 `posts` 문서를 색인합니다.
7. 관련 Redis cache를 prefix 기준으로 무효화합니다.
8. Controller는 `IEventPublisher`로 이벤트를 큐에 넣습니다.
9. `KafkaEventDispatchService`가 백그라운드에서 Kafka topic으로 발행합니다.

### 피드 조회

피드는 현재 사용자의 게시물과 팔로우한 사용자의 게시물을 포함합니다. 본인 게시물은 모든 visibility가 허용됩니다. 팔로우한 사용자의 게시물은 `Public`, `FollowersOnly`만 노출됩니다. `Private` 게시물은 다른 사용자의 피드에 노출되지 않습니다.

### 검색과 인기 검색어

검색 요청은 Elasticsearch 결과를 반환하고 `SearchEvent`를 큐에 넣습니다. Kafka, Logstash, Elasticsearch는 이벤트를 `jerrygram-events-search-YYYY.MM.DD`에 저장합니다. `PopularSearchService`는 `jerrygram-events-*`의 `searchTerm.keyword`를 aggregation하고, 최근 6시간과 이전 6시간을 비교해 trending 검색어를 계산합니다.

## 이벤트 파이프라인

| Topic | Producer | Consumer | 목적 |
| --- | --- | --- | --- |
| `post-events` | .NET API | Logstash / Elasticsearch | 게시물 생성, 좋아요, 댓글, 삭제 분석 |
| `user-events` | .NET API | Logstash / Elasticsearch | 회원가입, 로그인, 팔로우, 프로필 조회 분석 |
| `search-events` | .NET API | Logstash / Elasticsearch | 인기/트렌딩 검색어 분석 |

Controller는 Kafka 발행을 직접 기다리지 않습니다. `IEventPublisher`가 bounded channel에 이벤트를 넣고, hosted service가 `IEventService`를 통해 발행합니다. 그래서 요청 지연과 이벤트 발행 실패가 분리됩니다.

## 모델 커버리지

| Entity | 현재 포함 |
| --- | --- |
| `User` | username, email, password hash, profile image, created time, relations |
| `Post` | image URL, caption, visibility, author, comments, likes, saves, tags |
| `Comment` | content, author, post, created time |
| `Notification` | recipient, actor, type, post, message, read flag |
| Join models | follow, like, save, post-tag |

추가로 고려할 필드는 `UpdatedAt`, `DeletedAt`, `RowVersion`, `DisplayName`, `Bio`, `EmailVerifiedAt`, 이미지 메타데이터, moderation 상태, 대댓글, 추천 상호작용 weight입니다.

## 운영 메모

- 개발 환경에서는 시작 시 EF migration을 자동 적용할 수 있습니다.
- 운영 환경에서는 migration을 애플리케이션 시작과 분리하는 편이 안전합니다.
- runtime secret은 local config나 secret store에 두고 commit하지 않습니다.
- 게시물 삭제 시 Elasticsearch 문서도 함께 삭제합니다.
- Kafka/ELK가 꺼져 있어도 no-op 이벤트 서비스 경로로 핵심 API 동작은 유지할 수 있습니다.

## 검증

```powershell
$out = Join-Path $env:TEMP 'jerrygram-build-check-webapi'
dotnet build backend-dotnet/WebApi/WebApi.csproj -o $out
```
