# Jerrygram Java Backend

Language: [English](README.md) | [한국어](README.ko.md) | 日本語

Jerrygram の Java 21 + Spring Boot backend implementation です。主要な API surface を合わせ、PostgreSQL、Redis、Elasticsearch、Blob Storage oriented architecture を共有します。

React web UI は default で .NET API に接続します。この backend は Java/Spring Boot の alternate implementation として保持されています。

## Stack

- Java 21
- Spring Boot 3.2
- Spring Web MVC
- Spring Data JPA
- PostgreSQL
- Redis / Spring Cache
- Elasticsearch
- Spring Security + JWT
- Azure-compatible Blob Storage support
- Gradle

## Layers

```text
src/main/java/com/jerrygram/
├── presentation/       REST controllers
├── application/        commands, queries, DTOs, interfaces
├── domain/             entities, enums, value objects
├── infrastructure/     persistence, cache, search, blob, security
└── JerrygramApplication.java
```

## Local Environment

Local run の前に example file をコピーします。

```powershell
Copy-Item .env.example .env
```

Example values は root Docker stack と同じ adjusted local ports を使います。

- PostgreSQL: `localhost:15433`
- Redis: `localhost:16380`
- Elasticsearch: `http://localhost:19200`

## Commands

```powershell
.\gradlew.bat build
```

```powershell
.\gradlew.bat bootRun --args="--spring.profiles.active=dev"
```

bash-compatible shells:

```bash
./gradlew build
./gradlew bootRun --args="--spring.profiles.active=dev"
```

## API Areas

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

## Notes

- `.gradle/`, `build/`, `.idea/`, local `.env`, `application.log` は ignore 対象で、commit しません。
- `gradle/wrapper/gradle-wrapper.jar` は clone 後に wrapper が動作するよう intentional に tracked されています。
- Local app configuration files under `src/main/resources/application*.yml` are ignored. Use `.env.example` as the shared template.
