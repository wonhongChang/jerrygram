# Jerrygram Java バックエンド

Language: [English](README.md) | [한국어](README.ko.md) | 日本語

Jerrygram の Java 21 + Spring Boot バックエンド実装です。主要な API 領域を同じ形で持ち、PostgreSQL、Redis、Elasticsearch、Blob Storage を中心にした構成を共有します。

React Web UI はデフォルトで .NET API に接続するため、このバックエンドは Java 実装として維持しています。

## 技術スタック

- Java 21
- Spring Boot 3.2
- Spring Web MVC
- Spring Data JPA
- PostgreSQL
- Redis / Spring Cache
- Elasticsearch
- Spring Security + JWT
- Azure 互換 Blob Storage 対応
- Gradle

## レイヤー

```text
src/main/java/com/jerrygram/
|- presentation/       REST controllers
|- application/        commands, queries, DTOs, interfaces
|- domain/             entities, enums, value objects
|- infrastructure/     persistence, cache, search, blob, security
\- JerrygramApplication.java
```

## ローカル環境

実行前にサンプルファイルをコピーします。

```powershell
Copy-Item .env.example .env
```

サンプル設定はルート Docker スタックと同じ調整済みポートを使用します。

- PostgreSQL: `localhost:15433`
- Redis: `localhost:16380`
- Elasticsearch: `http://localhost:19200`

## コマンド

```powershell
.\gradlew.bat build
```

```powershell
.\gradlew.bat bootRun --args="--spring.profiles.active=dev"
```

bash 互換シェルでは次を使えます。

```bash
./gradlew build
./gradlew bootRun --args="--spring.profiles.active=dev"
```

## API 領域

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

## メモ

- `.gradle/`, `build/`, `.idea/`, ローカル `.env`, `application.log` は ignore 対象で、コミットしません。
- `gradle/wrapper/gradle-wrapper.jar` は clone 後に wrapper が動くよう意図的に追跡しています。
- `src/main/resources/application*.yml` 配下のローカル設定は ignore 対象です。共有テンプレートは `.env.example` を使います。
- CI は `./gradlew build` で Java smoke test を実行します。
