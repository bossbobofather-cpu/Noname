# GameHost Plugin

Host 시뮬레이션과 CQRS(Command/Result/Event) 인프라를 제공하는 공용 플러그인입니다.
Unity와 독립적으로 동작하는 순수 C# 런타임을 지향합니다.

## 핵심 구성
- **GameHostBase**: 시뮬레이션 루프와 큐 처리
- **GameCommandBase / GameCommandResultBase**: CQRS 메시지 기반
- **GameEventBase**: 호스트 이벤트
- **GameSnapshotBase**: 상태 스냅샷

## 폴더 구조
```
Runtime/
├── Application/   # 호스트 실행/루프/이벤트 버스/디스패치
└── Domain/        # 커맨드/이벤트/스냅샷 타입 정의
```

## 폴더별 설명
### Application
- 호스트 실행 루프(`GameHostBase`)와 큐 처리
- 결과/이벤트 디스패치, 스냅샷 생성/캐시
- GameEventBus 등 런타임 실행 흐름 담당

### Domain
- CQRS 메시지 베이스 타입
- 스냅샷/이벤트/커맨드 공용 인터페이스
- Host와 View 간 계약(타입 안전성) 정의

## 사용 흐름
1. Host 구현체에서 `HandleCommand`와 `BuildSnapshot`을 구현
2. `StartSimulation()`으로 호스트 루프 시작
3. 메인 스레드에서 `FlushEvents()`로 이벤트 디스패치

## 특징
- 스레드 안전 설계 (Queue + lock)
- Unity 의존성 없음
- Host/Client 분리 구조에 최적화
