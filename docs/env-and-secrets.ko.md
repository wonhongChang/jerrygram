# 환경 변수와 시크릿 설정

언어: [English](env-and-secrets.md) | 한국어 | [日本語](env-and-secrets.ja.md)

Jerrygram은 로컬 시크릿을 Git에 넣지 않습니다. `*.example` 파일만 커밋하고, 새 환경을 설정할 때 로컬 런타임 파일로 복사해 사용합니다.

## 로컬 파일

| 목적 | 예시 파일 | 로컬 파일 |
| --- | --- | --- |
| Docker 포트와 공통 시크릿 | `.env.example` | `.env` |
| .NET API 설정 | `backend-dotnet/WebApi/appsettings.example.json` | `backend-dotnet/WebApi/appsettings.json` |
| React 개발 서버 | `frontend-react/.env.example` | `frontend-react/.env` |
| Node 추천 서비스 | `jerrygram-recommend/.env.example` | `jerrygram-recommend/.env` |
| Java 백엔드 | `backend-java/.env.example` | `backend-java/.env` |

## 현재 로컬 포트

| 서비스 | 호스트 포트 |
| --- | ---: |
| React web | `13000` |
| .NET API | `5018` |
| 추천 서비스 | `13001` |
| PostgreSQL | `15433` |
| Redis | `16380` |
| Elasticsearch | `19200` |
| Kafka broker | `19092` |
| Kafka UI | `18081` |
| Kafka Connect | `18083` |
| Kibana | `15601` |
| Logstash API | `19600` |

## GitHub Actions

CI workflow는 production secret 없이 프로젝트를 build/test합니다. `OPENAI_API_KEY`, Azure storage credentials, deployment credentials 같은 런타임 전용 시크릿은 나중에 배포 workflow를 도입할 때 GitHub repository secrets로 추가합니다.

## 참고

- `JwtSettings:SecretKey`, DB password, Redis password, cloud storage connection string은 커밋된 설정에 넣지 않습니다.
- Docker 포트는 다른 로컬 프로젝트와 충돌을 줄이기 위해 일반 기본값에서 일부러 이동했습니다.
- 로컬 Blob Storage는 설정된 provider를 사용합니다. `UseDevelopmentStorage=true`는 Azurite 호환 로컬 스토리지를 대상으로 하며, 실제 Azure connection string도 코드 변경 없이 사용할 수 있습니다.
