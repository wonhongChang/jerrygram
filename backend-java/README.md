# Jerrygram Java Backend

Language: English | [한국어](README.ko.md) | [日本語](README.ja.md)

Java 21 + Spring Boot backend implementation for Jerrygram. It mirrors the main API surface and shares the same PostgreSQL, Redis, Elasticsearch, and Blob Storage oriented architecture.

The React web UI is wired to the .NET API by default, so this backend is kept as an alternate Java implementation.

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
|- presentation/       REST controllers
|- application/        commands, queries, DTOs, interfaces
|- domain/             entities, enums, value objects
|- infrastructure/     persistence, cache, search, blob, security
\- JerrygramApplication.java
```

## Local Environment

Copy the example file before running locally:

```powershell
Copy-Item .env.example .env
```

The example uses the same adjusted local service ports as the root Docker stack:

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

On bash-compatible shells:

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

- `.gradle/`, `build/`, `.idea/`, local `.env`, and `application.log` are ignored and should not be committed.
- `gradle/wrapper/gradle-wrapper.jar` is intentionally tracked so the wrapper works after clone.
- Local app configuration files under `src/main/resources/application*.yml` are ignored; use `.env.example` as the shared template.
- CI runs the Java smoke tests with `./gradlew build`.
