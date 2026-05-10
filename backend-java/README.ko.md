# Jerrygram Java 백엔드

언어: [English](README.md) | 한국어 | [日本語](README.ja.md)

Jerrygram의 Java 21 + Spring Boot 백엔드 구현입니다. 주요 API 표면을 비슷하게 제공하며 PostgreSQL, Redis, Elasticsearch, Blob Storage 중심 구조를 공유합니다.

React 웹 UI는 기본적으로 .NET API에 연결되어 있으므로, 이 백엔드는 Java 구현을 보여주는 대체 구현으로 유지합니다.

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

## 계층 구조

```text
src/main/java/com/jerrygram/
|- presentation/       REST controllers
|- application/        commands, queries, DTOs, interfaces
|- domain/             entities, enums, value objects
|- infrastructure/     persistence, cache, search, blob, security
\- JerrygramApplication.java
```

## 로컬 환경

실행 전에 예시 파일을 복사합니다.

```powershell
Copy-Item .env.example .env
```

예시 설정은 루트 Docker 스택과 동일한 조정 포트를 사용합니다.

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

bash 호환 셸에서는 다음을 사용할 수 있습니다.

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

## 참고

- `.gradle/`, `build/`, `.idea/`, 로컬 `.env`, `application.log`는 ignore 대상이며 커밋하지 않습니다.
- `gradle/wrapper/gradle-wrapper.jar`는 clone 후 wrapper가 동작하도록 의도적으로 추적합니다.
- `src/main/resources/application*.yml` 아래 로컬 앱 설정은 ignore 대상입니다. 공유 템플릿은 `.env.example`을 사용합니다.
- CI는 `./gradlew build`로 Java smoke test를 실행합니다.
