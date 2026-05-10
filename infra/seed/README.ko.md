# Jerrygram Seed 데이터

언어: [English](README.md) | 한국어 | [日本語](README.ja.md)

이 폴더에는 피드 랭킹, Blob 기반 이미지 업로드, Kafka 검색 이벤트, 추천 동작을 보여주기 위한 작은 데모 데이터 세트가 들어 있습니다.

## 구성

| 경로 | 목적 |
| --- | --- |
| `seed-data.json` | 사용자, 게시물, 팔로우, 좋아요, 저장, 반복 검색어 |
| `images/kafka-trend.png` | Kafka/검색 트렌드 데모용 게시물 이미지 |
| `images/blob-storage.png` | 로컬 Blob Storage 업로드 데모용 게시물 이미지 |
| `images/recommendation-loop.png` | 추천 이벤트 데모용 게시물 이미지 |
| `seed-jerrygram.ps1` | API 기반 반복 실행 가능한 seed 스크립트 |
| `verify-jerrygram-demo.ps1` | 추천, Kafka, Elasticsearch 동작 증거 확인 스크립트 |

## 권장 데모 흐름

1. `seed-data.json`의 사용자를 등록합니다.
2. 각 게시물 이미지와 caption, visibility를 맞춰 업로드합니다.
3. 팔로우 관계를 생성합니다.
4. 좋아요와 저장 interaction을 적용합니다.
5. 검색어를 순서대로 실행해 `SearchPerformed` 이벤트가 충분히 쌓이게 합니다.

현재 E2E 테스트도 같은 시나리오 형태의 mocked API response를 사용합니다. 그래서 전체 Docker 스택 없이도 CI에서 UI를 검증할 수 있습니다.

## Seed 스크립트 실행

Docker 스택과 .NET API를 먼저 실행한 뒤:

```powershell
.\infra\seed\seed-jerrygram.ps1
```

Windows 실행 정책 때문에 막히면 현재 프로세스에만 우회 옵션을 적용해 실행할 수 있습니다.

```powershell
powershell -ExecutionPolicy Bypass -File .\infra\seed\seed-jerrygram.ps1
```

스크립트는 먼저 `seed-data.json`의 username/email을 사용합니다. 로컬 DB에 같은 계정이 다른 비밀번호로 이미 있으면 기본 `_seed` suffix를 붙인 계정, 예를 들어 `jerry_seed`, 으로 자동 재시도합니다. 필요하면 `-CollisionSuffix`로 바꿀 수 있습니다.

seed 데이터가 들어간 뒤 live demo 요약을 확인합니다.

```powershell
.\infra\seed\verify-jerrygram-demo.ps1
```

```powershell
powershell -ExecutionPolicy Bypass -File .\infra\seed\verify-jerrygram-demo.ps1
```
