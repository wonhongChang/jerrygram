# Elasticsearch 인덱스 목록과 정리 기준

언어: [English](elasticsearch-indexes.md) | 한국어 | [日本語](elasticsearch-indexes.ja.md)

이 문서는 로컬 Kibana/Elasticsearch UI에 보이는 인덱스를 분류합니다. 기준은 2026-05-10에 확인한 로컬 스택입니다.

## 유지 대상

| Pattern / index | 이유 |
| --- | --- |
| `.internal.alerts-*`, `.kibana*` | Kibana 시스템 인덱스와 alerting 내부 인덱스입니다. 일반 정리 중 삭제하지 않는 편이 좋습니다. |
| `posts` | 게시물 검색과 탐색에 쓰이는 앱 검색 인덱스입니다. |
| `users` | 사용자 검색에 쓰이는 앱 검색 인덱스입니다. |
| `tags` | 해시태그 검색에 쓰이는 앱 검색 인덱스입니다. |
| `jerrygram-events-post-*` | 게시물 이벤트 분석을 보여주는 Kafka/Logstash 증거 인덱스입니다. |
| `jerrygram-events-user-*` | 사용자 이벤트 분석을 보여주는 Kafka/Logstash 증거 인덱스입니다. |
| `jerrygram-events-search-*` | 인기/트렌딩 검색어 계산에 쓰이는 Kafka/Logstash 증거 인덱스입니다. |

## 정리 후보

| Index | 현재 관찰 | 권장 |
| --- | --- | --- |
| `jerrygram-logs-000001` | 문서 `0`개, alias `jerrygram-logs` | Logstash 로그 alias 데모가 필요 없다면 로컬 정리 후보입니다. |
| `jerrygram-dotnet-backend-2025.07.29` | 오래된 앱 로그 인덱스, 문서 `1`개 | 과거 로그 증거가 필요 없으면 로컬에서 archive/delete 후보입니다. |
| `jerrygram-java-backend-2025.10.23` | 오래된 Java 백엔드 로그 인덱스, 문서 `1`개 | Java 로그 이력이 디버깅이나 감사 이력에 필요할 때만 유지합니다. |
| `jerrygram-java-backend-2026.05.09` | 최근 Java 백엔드 로그 인덱스, 문서 `3`개 | Java 백엔드를 보여줄 때 유용하지만, 현재 React + .NET 실행 경로에는 필수는 아닙니다. |

## Yellow 상태가 보이는 이유

로컬 Elasticsearch는 단일 노드입니다. replica가 있는 인덱스는 다른 노드에 replica shard를 배정할 수 없어 `yellow`로 보일 수 있습니다. 로컬 개발 환경에서는 정상 범위이며 앱이 깨졌다는 뜻은 아닙니다.

## 확인 명령

앱/이벤트 인덱스 확인:

```powershell
Invoke-RestMethod "http://localhost:19200/_cat/indices?format=json&h=health,status,index,docs.count,store.size,creation.date.string"
```

alias 확인:

```powershell
Invoke-RestMethod "http://localhost:19200/_cat/aliases?format=json&h=alias,index"
```

로컬 정리 예시입니다. 로컬 증거 데이터를 정말 삭제하려는 경우에만 실행합니다.

```powershell
Invoke-RestMethod -Method Delete "http://localhost:19200/jerrygram-logs-000001"
```

일반 데모 중에는 `.internal.*`, `.kibana*`, `posts`, `users`, `tags`, `jerrygram-events-*`는 삭제하지 마세요.
