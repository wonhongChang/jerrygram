# Jerrygram Seed 데이터

언어: [English](README.md) | 한국어 | [日本語](README.ja.md)

이 폴더는 피드 랭킹, Blob 기반 이미지 업로드, Kafka 검색 이벤트, 추천 동작을 보여주기 위한 작은 데모 데이터 세트입니다.

## 구성

| 경로 | 목적 |
| --- | --- |
| `seed-data.json` | 사용자, 게시물, 팔로우, 좋아요, 저장, 반복 검색어 |
| `images/kafka-trend.png` | Kafka/검색 트렌드 데모용 게시물 이미지 |
| `images/blob-storage.png` | 로컬 Blob Storage 업로드 데모용 게시물 이미지 |
| `images/recommendation-loop.png` | 추천 이벤트 데모용 게시물 이미지 |

## 권장 데모 흐름

1. `seed-data.json`의 사용자를 등록합니다.
2. 각 게시물 이미지와 caption, visibility를 맞춰 업로드합니다.
3. follow 관계를 생성합니다.
4. like와 save interaction을 적용합니다.
5. 검색어를 순서대로 실행해 `SearchPerformed` 이벤트가 충분히 쌓이게 합니다.

현재 E2E 테스트도 같은 시나리오 형태의 mocked API response를 사용합니다. 그래서 전체 Docker 스택 없이도 CI에서 UI를 검증할 수 있습니다.
