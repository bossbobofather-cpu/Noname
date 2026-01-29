# GameHost Plugin

Host 시뮬레이션과 CQRS(Command/Result/Event) 인프라를 제공하는 공용 플러그인입니다.
Unity와 독립적으로 동작하는 순수 C# 런타임을 지향합니다.

## 핵심 구성
- **GameHostBase**: 시뮬레이션 루프, 큐 처리, 스냅샷 캐시
- **GameCommandBase / GameCommandResultBase**: CQRS 메시지 베이스
- **GameEventBase**: 호스트 이벤트 베이스
- **GameSnapshotBase**: 상태 스냅샷 베이스

## 폴더 구조
```
Runtime/
├── Application/   # 호스트 실행/루프/이벤트 버스/디스패치
└── Domain/        # 커맨드/이벤트/스냅샷 타입 정의
```

## Application 폴더
- `GameHostBase` : 호스트 시뮬레이션 루프와 큐 처리
- 이벤트/결과 디스패치 및 스냅샷 캐시 관리
- 스레드 안전성과 메인 스레드 디스패치 흐름 제공

## Domain 폴더
- CQRS 메시지 베이스 타입 정의
- 스냅샷/이벤트/커맨드 공용 계약 제공
- Host와 View 간 타입 안전성 보장

## 사용 흐름
1. Host 구현체에서 `HandleCommand`와 `BuildSnapshotInternal` 구현
2. `StartSimulation()`으로 호스트 루프 시작
3. 메인 스레드에서 `FlushEvents()`로 이벤트 디스패치

## 특징
- 스레드 안전 설계 (Queue + lock)
- Unity 의존성 없음
- Host/Client 분리 구조에 최적화
