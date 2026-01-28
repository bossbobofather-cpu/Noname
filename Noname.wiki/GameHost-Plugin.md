# GameHost 플러그인

Host 시뮬레이션과 CQRS(Command/Result/Event) 인프라를 제공하는 공용 플러그인입니다. Unity와 독립적으로 동작하는 순수 C# 런타임을 지향합니다.

## 핵심 구성

| Name | Description |
|---|---|
| GameHostBase | 시뮬레이션 루프, 큐 처리, 스냅샷 생성 |
| GameCommandBase | Command 베이스 |
| GameCommandResultBase | Command 처리 결과 베이스 |
| GameEventBase | Host 이벤트 베이스 |
| GameSnapshotBase | 상태 스냅샷 베이스 |
| GameEventBus | 전역/씬 스코프 이벤트 버스 |

## 사용 흐름

1. Host 구현체에서 `HandleCommand`와 `BuildSnapshot`을 구현
2. `StartSimulation()`으로 호스트 루프 시작
3. 메인 스레드에서 `FlushEvents()`로 이벤트 디스패치

## 특징
- 스레드 안전 설계 (Queue + lock)
- Unity 의존성 최소화
- Host/Client 분리 구조에 최적화

## 참고
- GameHost는 플러그인으로 분리되어 프로젝트 공용 인프라로 사용됩니다.
