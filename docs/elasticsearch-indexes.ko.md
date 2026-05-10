# Elasticsearch 인덱스 목록

언어: [English](elasticsearch-indexes.md) | 한국어 | [日本語](elasticsearch-indexes.ja.md)

이 문서는 로컬 Kibana/Elasticsearch UI에서 보이는 인덱스를 분류합니다. 기준은 2026-05-10에 확인한 로컬 스택입니다.

## 유지 대상

| 패턴 / 인덱스 | 이유 |
| --- | --- |
| `.internal.alerts-*`, `.kibana*` | Kibana 시스템 인덱스와 alerting 내부 인덱스입니다. 일반 정리 중 삭제하지 않습니다. |
| `posts` | 게시물 검색과 탐색에 사용하는 활성 앱 검색 인덱스입니다. 현재 로컬 문서 수: `16`. |
| `users` | 사용자 검색에 사용하는 활성 앱 검색 인덱스입니다. 현재 로컬 문서 수: `18`. |
| `tags` | 해시태그 검색에 사용하는 활성 앱 검색 인덱스입니다. 현재 로컬 문서 수: `9`. |
| `jerrygram-events-post-*` | 게시물 analytics를 위한 Kafka/Logstash 이벤트 증거입니다. 현재 로컬 문서 수는 2026-05-09 `12`, 2026-05-10 `5`입니다. |
| `jerrygram-events-user-*` | 사용자 analytics를 위한 Kafka/Logstash 이벤트 증거입니다. 현재 로컬 문서 수는 2026-05-09 `45`, 2026-05-10 `12`입니다. |
| `jerrygram-events-search-*` | 인기/트렌딩 검색어를 위한 Kafka/Logstash 이벤트 증거입니다. 현재 로컬 문서 수는 2026-05-09 `8`, 2026-05-10 `8`입니다. |

## 정리 후보

| 인덱스 | 현재 관찰 | 권장 |
| --- | --- | --- |
| `jerrygram-logs-000001` | 문서 `0`개, alias `jerrygram-logs` | Logstash 로그 alias를 데모하지 않는다면 로컬에서 정리해도 됩니다. |
| `jerrygram-dotnet-backend-2025.07.29` | 오래된 .NET 앱 로그 인덱스, 문서 `1`개 | 과거 로그 증거가 필요하면 유지하고, 아니면 로컬에서 archive/delete 가능합니다. |
| `jerrygram-java-backend-2025.10.23` | 오래된 Java 백엔드 로그 인덱스, 문서 `1`개 | 과거 Java 로그 증거가 필요할 때만 유지합니다. |
| `jerrygram-java-backend-2026.05.09` | 최근 Java 백엔드 로그 인덱스, 문서 `3`개 | 선택 사항입니다. Java 백엔드를 시연할 때는 유용하지만 현재 React + .NET 기본 경로에는 필수는 아닙니다. |

## 일부 인덱스가 yellow인 이유

로컬 Elasticsearch는 single-node 구성입니다. replica shard가 다른 노드에 배치될 수 없어 replica가 있는 인덱스는 `yellow`로 보일 수 있습니다. 로컬에서는 정상적인 상태이며 앱이 고장났다는 뜻은 아닙니다.

## 유용한 명령어

앱/이벤트 인덱스 확인:

```powershell
Invoke-RestMethod "http://localhost:19200/_cat/indices?format=json&h=health,status,index,docs.count,store.size,creation.date.string"
```

alias 확인:

```powershell
Invoke-RestMethod "http://localhost:19200/_cat/aliases?format=json&h=alias,index"
```

로컬 정리 예시입니다. 로컬 증거 데이터를 일부러 지우려는 경우에만 실행하세요.

```powershell
Invoke-RestMethod -Method Delete "http://localhost:19200/jerrygram-logs-000001"
```

일반 데모 중에는 `.internal.*`, `.kibana*`, `posts`, `users`, `tags`, `jerrygram-events-*`를 삭제하지 마세요.
