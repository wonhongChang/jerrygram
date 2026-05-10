# Jerrygram Java 백엔드

언어: [English](README.md) | 한국어 | [日本語](README.ja.md)

Jerrygram의 Java 21 + Spring Boot 백엔드 구현입니다. 주요 API 영역을 맞추고 PostgreSQL, Redis, Elasticsearch, Blob Storage 중심의 아키텍처를 공유합니다.

React 웹 UI는 기본적으로 .NET API에 연결됩니다. 이 백엔드는 Java/Spring Boot 대체 구현으로 유지됩니다.

## 기술 스택

- Java 21
- Spring Boot 3.2
- Spring Web MVC
- Spring Data JPA
- PostgreSQL
- Redis / Spring Cache
- Elasticsearch
- Spring Security + JWT
- Azure 호환 Blob Storage 지원
- Gradle

## 레이어

```text
src/main/java/com/jerrygram/
├── presentation/       REST controllers
├── application/        commands, queries, DTOs, interfaces
├── domain/             entities, enums, value objects
├── infrastructure/     persistence, cache, search, blob, security
└── JerrygramApplication.java
```

## 로컬 환경

로컬 실행 전에 예시 파일을 복사합니다.

```powershell
Copy-Item .env.example .env
```

예시 값은 루트 Docker 스택과 같은 조정된 로컬 포트를 사용합니다.

- PostgreSQL: `localhost:15433`
- Redis: `localhost:16380`
- Elasticsearch: `http://localhost:19200`

## 명령어

```powershell
.\gradlew.bat build
```

```powershell
.\gradlew.bat bootRun --args="--spring.profiles.active=dev"
```

bash 호환 셸에서는 다음처럼 실행합니다.

```bash
./gradlew build
./gradlew bootRun --args="--spring.profiles.active=dev"
```

## API 영역

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/posts`
- `POST /api/posts`
- `GET /api/posts/{id}`
- `POST /api/posts/{id}/like`
- `GET /api/explore`
- `GET /api/search`
- `GET /api/search/autocomplete`
- `GET /api/users/me`
- `GET /api/users/{username}`

## 메모

- `.gradle/`, `build/`, `.idea/`, 로컬 `.env`, `application.log`는 ignore 대상이며 커밋하지 않습니다.
- `gradle/wrapper/gradle-wrapper.jar`는 clone 후 wrapper가 동작하도록 의도적으로 추적합니다.
- `src/main/resources/application*.yml`의 로컬 설정 파일은 ignore 대상입니다. 공유 템플릿은 `.env.example`을 사용합니다.
