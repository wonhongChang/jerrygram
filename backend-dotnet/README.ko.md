# Jerrygram .NET 백엔드

언어: [English](README.md) | 한국어 | [日本語](README.ja.md)

Jerrygram의 ASP.NET Core Web API입니다. React 웹 UI가 기본적으로 사용하는 백엔드입니다.

## 기술 스택

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Redis 캐시와 인메모리 fallback
- Elasticsearch 검색
- Kafka 이벤트 발행
- Azure 호환 Blob Storage 지원
- xUnit 테스트

## 프로젝트 구조

```text
backend-dotnet/
├── Application/            commands, queries, DTOs, interfaces
├── Domain/                 entities, constants, value objects
├── Persistence/            EF Core DbContext, migrations, repositories
├── Infrastructure/         auth, cache, blob, search, Kafka, recommendation clients
├── WebApi/                 controllers, middleware, validators, startup
├── Domain.Tests/           domain behavior tests
├── Infrastructure.Tests/   infrastructure query tests
└── Jerrygram/              Visual Studio solution
```

## 로컬 설정

로컬 실행 전에 예시 설정 파일을 복사합니다.

```powershell
Copy-Item WebApi/appsettings.example.json WebApi/appsettings.json
```

커밋된 Docker 설정은 루트 compose 파일의 조정된 로컬 포트를 사용합니다.

## 명령어

저장소 루트에서 실행합니다.

```powershell
dotnet restore backend-dotnet/WebApi/WebApi.csproj
dotnet build backend-dotnet/WebApi/WebApi.csproj --configuration Release
dotnet test backend-dotnet/Domain.Tests/Domain.Tests.csproj --configuration Release
dotnet test backend-dotnet/Infrastructure.Tests/Infrastructure.Tests.csproj --configuration Release
```

API 실행:

```powershell
dotnet run --project backend-dotnet/WebApi/WebApi.csproj --urls http://localhost:5018
```

## API 영역

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/posts`
- `POST /api/posts`
- `GET /api/posts/feed`
- `GET /api/explore`
- `GET /api/search`
- `GET /api/search/popular`
- `GET /api/search/popular/trending`
- `GET /api/users/me`
- `GET /api/users/{username}`
- `GET /api/notifications`

## 메모

- `bin/`, `obj/`, `*.user`, 로컬 `appsettings.json`, `appsettings.Development.json`는 ignore 대상이며 커밋하지 않습니다.
- `appsettings.example.json`은 공유 로컬 템플릿입니다.
- API가 Kafka 이벤트를 발행하고, 로컬 분석 파이프라인이 이를 Elasticsearch로 적재합니다.
