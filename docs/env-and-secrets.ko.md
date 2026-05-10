# 환경 변수와 Secret 설정

언어: [English](env-and-secrets.md) | 한국어 | [日本語](env-and-secrets.ja.md)

Jerrygram은 로컬 secret을 Git에 올리지 않습니다. `*.example` 파일만 commit하고, 각 개발 환경에서는 local runtime 파일로 복사해서 사용합니다.

## 로컬 파일

| 목적 | 예시 파일 | 로컬 파일 |
| --- | --- | --- |
| Docker 포트와 공통 secret | `.env.example` | `.env` |
| .NET API 설정 | `backend-dotnet/WebApi/appsettings.example.json` | `backend-dotnet/WebApi/appsettings.json` |
| React dev server | `frontend-react/.env.example` | `frontend-react/.env` |
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

현재 CI workflow는 운영 secret 없이 build/test만 수행합니다. `OPENAI_API_KEY`, Azure storage credential, 배포 credential 같은 runtime secret은 실제 배포 workflow를 추가할 때 GitHub repository secrets로 넣는 편이 좋습니다.

## 메모

- `JwtSettings:SecretKey`, DB password, Redis password, cloud storage connection string은 commit하지 않습니다.
- Docker 포트는 다른 로컬 프로젝트와 충돌을 줄이기 위해 기본값에서 옮겨두었습니다.
- 로컬 Blob Storage는 설정된 provider를 사용합니다. `UseDevelopmentStorage=true`는 Azurite 호환 로컬 저장소를 가리키며, 실제 Azure connection string도 코드 변경 없이 사용할 수 있습니다.
