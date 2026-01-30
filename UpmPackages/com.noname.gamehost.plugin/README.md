# GameHost 플러그인

## 개요
GameHost는 멀티스레드 시뮬레이션과 CQRS 패턴을 기반으로, 호스트 로직을 Unity에서 분리해 운용할 수 있도록 만든 경량 런타임입니다.
게임 진행 로직은 Host에서 처리하고, View는 Command 전송과 Result/Event 수신만 담당하는 흐름을 목표로 합니다.

## 핵심 구성
| Name | Description |
|---|---|
| `GameHostBase<TCommand, TResult, TEvent, TSnapshot>` | 호스트 루프, 커맨드 처리, 결과/이벤트 큐 관리 |
| `GameCommandBase` | 커맨드 베이스 |
| `GameCommandResultBase` | 커맨드 결과 베이스 |
| `GameEventBase` | 이벤트 베이스 |
| `GameSnapshotBase` | 스냅샷 베이스 |
| `IHostCommandBus<TCommand, TResult, TEvent, TSnapshot>` | View가 접근하는 커맨드 버스 인터페이스 |

## 폴더 구조
```
Runtime/
├── Application/   # 호스트 실행/루프/이벤트 버스/디스패치
└── Domain/        # 커맨드/이벤트/스냅샷 타입 정의
```

## Application 폴더
- `GameHostBase` : 호스트 시뮬레이션 루프와 큐 처리
- 결과/이벤트 디스패치 및 스냅샷 캐시 관리
- 스레드 안전성과 메인 스레드 디스패치 흐름 제공

## Domain 폴더
- CQRS 메시지 베이스 타입 정의
- 스냅샷/이벤트/커맨드 공용 계약 제공
- Host와 View 간 타입 안전성 보장

## 기본 흐름
1. Host 인스턴스를 생성하고 `StartSimulation()`을 호출합니다.
2. View는 `IHostCommandBus`를 통해 커맨드를 전송합니다.
3. Host는 Result/Event를 큐에 쌓고, 메인 스레드에서 `FlushEvents()`로 디스패치합니다.
4. 필요 시 `BuildSnapshot()`으로 상태 스냅샷을 생성합니다.

## 특징
- 스레드 안전 설계 (Queue + lock)
- Unity 의존성 없음
- Host/Client 분리 구조에 최적화
