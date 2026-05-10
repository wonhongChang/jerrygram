# Jerrygram 시드 데이터

언어: [English](README.md) | 한국어 | [日本語](README.ja.md)

이 폴더는 피드 랭킹, Blob Storage 이미지, Kafka 검색 이벤트, 추천 동작을 보여주기 위한 작은 데이터 세트를 포함합니다.

## 구성

| 경로 | 목적 |
| --- | --- |
| `seed-data.json` | 사용자, 게시물, 팔로우, 좋아요, 저장, 반복 검색어 |
| `images/kafka-trend.png` | Kafka/검색 트렌드 데모용 게시물 이미지 |
| `images/blob-storage.png` | 로컬 Blob Storage 업로드 데모용 게시물 이미지 |
| `images/recommendation-loop.png` | 추천 이벤트 데모용 게시물 이미지 |
| `seed-jerrygram.ps1` | 반복 실행 가능한 API 시드 스크립트 |
| `verify-jerrygram-demo.ps1` | 추천, Kafka, Elasticsearch 데모 증거 확인 스크립트 |

## 추천 데모 흐름

1. `seed-data.json`의 사용자를 등록합니다.
2. 각 게시물 이미지를 캡션과 공개 범위에 맞춰 업로드합니다.
3. 팔로우 관계를 만듭니다.
4. 좋아요와 저장을 적용합니다.
5. 인기/트렌딩 검색 화면에 충분한 이벤트가 생기도록 검색어를 순서대로 실행합니다.

현재 E2E 테스트는 같은 시나리오 형태를 mock API 응답으로 검증하므로, CI에서는 전체 Docker 스택 없이도 UI를 확인할 수 있습니다.

## 시드 스크립트 실행

먼저 Docker 스택과 .NET API를 실행한 뒤 다음을 실행합니다.

```powershell
.\infra\seed\seed-jerrygram.ps1
```

Windows에서 스크립트 실행이 막히면 프로세스 단위 우회 옵션으로 실행합니다.

```powershell
powershell -ExecutionPolicy Bypass -File .\infra\seed\seed-jerrygram.ps1
```

스크립트는 먼저 `seed-data.json`의 사용자명과 이메일을 사용합니다. 로컬 DB에 같은 계정이 다른 인증 정보로 이미 있으면 기본 `_seed` suffix를 붙여 다시 시도합니다. 예: `jerry_seed`. `-CollisionSuffix`로 변경할 수 있습니다.

시드 데이터 적재 후 라이브 데모 요약을 확인합니다.

```powershell
.\infra\seed\verify-jerrygram-demo.ps1
```

```powershell
powershell -ExecutionPolicy Bypass -File .\infra\seed\verify-jerrygram-demo.ps1
```
