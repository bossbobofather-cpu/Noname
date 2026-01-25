# ExploreGame - 텍스트 로그라이크 던전 탐험 게임

카피바라 Go 스타일의 자동 진행 던전 탐험 게임입니다. 텍스트 로그로 진행 상황을 확인할 수 있습니다.

## 특징

- **자동 던전 탐험**: 2초마다 자동으로 다음 층으로 이동
- **자동 전투**: 1초마다 턴제 전투 자동 진행
- **레벨업 시스템**: 경험치 획득으로 자동 레벨업
- **실시간 로그**: Debug.Log로 모든 진행 상황 출력
- **Host-Client 분리**: GameHostBase 기반 멀티스레드 시뮬레이션

## 아키텍처

Clean Architecture + DDD 패턴을 따르며, Host-Client 분리를 통해 멀티스레드 시뮬레이션을 지원합니다.

### 레이어 구조

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│  (Unity 메인 스레드, GameMode, Modules, ViewModel)           │
├─────────────────────────────────────────────────────────────┤
│                    Application Layer                        │
│  (Host 스레드, CQRS, 게임 로직, Module Hosts)                │
├─────────────────────────────────────────────────────────────┤
│                      Domain Layer                           │
│  (순수 게임 상태, 비즈니스 로직, 스레드 안전)                  │
├─────────────────────────────────────────────────────────────┤
│                       Data Layer                            │
│  (설정, ScriptableObject, 외부 데이터)                       │
└─────────────────────────────────────────────────────────────┘
```

### 상세 컴포넌트 구조

#### Domain Layer (순수 비즈니스 로직)
```
Domain/
├── ExploreCharacterState.cs
│   └── 캐릭터 상태 관리 (HP, 공격력, 방어력, 골드, 경험치, 레벨업)
├── ExploreMonsterState.cs
│   └── 몬스터 상태 (HP, 공격력, 방어력, 보상)
├── ExploreDungeonState.cs
│   └── 던전 상태 (5층 구조, 스테이지 진행, DungeonStageData)
├── ExploreCombatState.cs
│   └── 전투 상태 (턴 관리, 몬스터 목록, 승패 판정)
├── ExploreHostState.cs
│   └── 전체 상태 컨테이너 (스레드 안전, lock 기반)
└── ExploreStateSnapshot.cs
    └── 불변 스냅샷 (캐릭터/던전/전투 스냅샷 포함)
```

#### Application Layer (게임 로직 & Host)
```
Application/Host/
├── ExploreHost.cs
│   └── GameHostBase<Command, Result, Event, Snapshot> 상속
│       ├── 별도 스레드에서 30 FPS 시뮬레이션 실행
│       ├── ConcurrentQueue로 커맨드 처리
│       └── 0.1초마다 스냅샷 생성
├── ExploreModeHost.cs
│   └── 게임 로직 총괄
│       ├── 커맨드 라우팅 (Join, StartDungeon)
│       ├── Module Host 관리
│       └── Tick 전파 및 Snapshot 빌드
├── ExploreHostTypes.cs
│   └── CQRS 타입 정의
│       ├── Commands: ExploreJoinCommand, ExploreStartDungeonCommand
│       ├── Results: ExploreJoinResult, ExploreStartDungeonResult
│       └── Events: 10가지 게임 이벤트
└── Modules/
    ├── ExploreDungeonModuleHost.cs
    │   └── 던전 자동 진행 (2초마다 스테이지 이동)
    └── ExploreCombatModuleHost.cs
        └── 자동 전투 (1초마다 턴 진행, 데미지 계산, 보상 지급)
```

#### Presentation Layer (Unity 컴포넌트)
```
Presentation/
├── ExploreMode.cs (MonoBehaviour, GameMode 상속)
│   └── Host 생성 및 관리
│       ├── StartSimulation() / StopSimulation()
│       ├── FlushEvents() (메인 스레드에서 이벤트 디스패치)
│       ├── ViewModel 업데이트 (0.1초마다 Snapshot 적용)
│       └── 자동 Join 및 던전 시작
├── ViewModel/
│   └── ExploreViewModel.cs
│       └── UI 바인딩용 데이터 (Snapshot → UI 변환)
├── Host/
│   └── ExploreHostEventRaisedEvent.cs
│       └── Host 이벤트 → GameEventBus 브릿지
└── Modules/ (MonoBehaviour, ModuleBase 상속)
    ├── TextLogModule.cs
    │   └── GameEventBus 구독 → Debug.Log 색상 출력
    └── StatusDisplayModule.cs
        └── 5초마다 캐릭터 상태 요약 출력
```

#### Data Layer (설정)
```
Data/
└── ExploreHostConfig.cs
    └── 게임 설정값 (전투/던전 간격, 초기 스탯)
```

### 데이터 흐름

```
[Unity 메인 스레드]                [Host 시뮬레이션 스레드]
       │                                    │
ExploreMode ──Submit(Command)──> ConcurrentQueue
       │                                    │
       │                              HandleCommand()
       │                                    │
       │                             ExploreModeHost
       │                                    │
       │                         ┌──────────┴─────────┐
       │                    DungeonModule      CombatModule
       │                         │                    │
       │                    OnTick(Δt)           OnTick(Δt)
       │                         │                    │
       │                    [State 변경]         [State 변경]
       │                         │                    │
       │                    PublishEvent()       PublishEvent()
       │                                    │
FlushEvents() <──── ConcurrentQueue  <──────┘
       │
  EventRaised
       │
GameEventBus ──> TextLogModule ──> Debug.Log
       │
BuildSnapshot() ──────────────────> Snapshot
       │
ExploreViewModel.ApplySnapshot()
```

### 핵심 디자인 패턴

#### 1. CQRS (Command Query Responsibility Segregation)
- **Command**: 상태 변경 요청 (ExploreJoinCommand, ExploreStartDungeonCommand)
- **Result**: 커맨드 처리 결과 (Success/Failure)
- **Event**: 상태 변경 알림 (ExploreDungeonStartedEvent 등)
- **Query**: Snapshot을 통한 상태 조회

#### 2. Snapshot Pattern
- 불변 객체로 특정 시점의 게임 상태 캡처
- 스레드 간 안전한 데이터 전송
- Tick 기반 시간 동기화

#### 3. Module Pattern
- 게임 로직을 독립적인 Module로 분리
- Host Module (Application): DungeonModule, CombatModule
- Presentation Module: TextLogModule, StatusDisplayModule
- 생명주기: Initialize → Startup → Tick → Shutdown

#### 4. Event-Driven Architecture
- Host 이벤트 발행 → GameEventBus → Module 구독
- 느슨한 결합으로 확장성 향상
- ExploreHostEventRaisedEvent로 Host-Presentation 브릿지

### 스레드 안전성

- **Host 스레드**: ExploreHost의 RunLoop() 메서드에서 독립 실행 (30 FPS)
- **State 보호**: ExploreHostState의 모든 public 메서드는 `lock (_stateLock)` 사용
- **커맨드 큐**: ConcurrentQueue로 스레드 안전 커맨드 제출
- **이벤트 디스패치**: ConcurrentQueue → FlushEvents() (메인 스레드)
- **Snapshot**: 불변 객체로 스레드 간 안전한 데이터 전송
- **Combat/Dungeon State**: 내부 lock으로 멀티스레드 안전성 보장

## Unity에서 사용하기

### 1. 씬 설정

1. 빈 GameObject 생성 → `ExploreGameMode` 이름 변경
2. `ExploreMode` 컴포넌트 추가
3. Inspector 설정:
   - Local User Id: `1` (플레이어 ID)
   - Character Name: `용사` (캐릭터 이름)
   - Snapshot Interval: `0.1` (스냅샷 갱신 간격)
   - Auto Start Dungeon: `true` (자동 시작)
   - Start Dungeon Id: `dungeon_1` (던전 ID)

### 2. 모듈 추가

**TextLogModule** (필수):
1. `ExploreGameMode` 하위에 빈 GameObject 생성
2. `TextLogModule` 컴포넌트 추가
3. Inspector에서 색상 설정:
   - Event Color: Cyan
   - Combat Color: Yellow
   - Reward Color: Green

**StatusDisplayModule** (선택):
1. `ExploreGameMode` 하위에 빈 GameObject 생성
2. `StatusDisplayModule` 컴포넌트 추가
3. Update Interval: `5` (5초마다 상태 출력)

### 3. 실행

1. Play 버튼 클릭
2. Console 창에서 로그 확인

## 콘솔 출력 예시

```
[게임 시작] '용사'이(가) 탐험을 시작했습니다!
[던전 시작] dungeon_1 던전 입장! (총 5층)
[층 이동] 1층에 도착했습니다.
[전투 시작] 슬라임 1마리가 나타났습니다!
용사이(가) 슬라임에게 10 데미지!
슬라임이(가) 용사에게 3 데미지!
용사이(가) 슬라임에게 10 데미지! [사망]
[승리] 전투에서 승리했습니다!
[보상] 골드 +10, 경험치 +20
[층 이동] 2층에 도착했습니다.
[전투 시작] 고블린 2마리가 나타났습니다!
...
[레벨업] 레벨 2로 레벨업했습니다!
...
[던전 완료] dungeon_1 던전을 완료했습니다!

[상태] [용사] Lv.2 HP:120/120 ATK:12 DEF:6 Gold:150 | 던전:dungeon_1 (완료)
```

## 게임 흐름

1. **참가**: Join 커맨드 → 캐릭터 생성
2. **던전 시작**: StartDungeon 커맨드
3. **자동 탐험**: 2초마다 다음 층으로 이동
4. **몬스터 조우**: 각 층에 몬스터 등장
5. **자동 전투**:
   - 1초마다 턴 진행
   - 플레이어 공격 → 몬스터 공격
   - 데미지 = Max(1, 공격력 - 방어력/2)
6. **보상**: 승리 시 골드 + 경험치
7. **레벨업**: 경험치 100 * Level 달성 시
8. **반복**: 5층까지 진행
9. **결과**: 던전 완료 or 전투 패배

## 던전 구조 (하드코딩)

| 층 | 몬스터 | 레벨 | 수량 |
|---|--------|------|------|
| 1 | 슬라임 | 1 | 1마리 |
| 2 | 고블린 | 2 | 2마리 |
| 3 | 오크 | 3 | 1마리 |
| 4 | 트롤 | 4 | 2마리 |
| 5 | 드래곤 | 5 | 1마리 |

## 커맨드 사용 예시

```csharp
// ExploreMode 참조
var exploreMode = FindObjectOfType<ExploreMode>();

// 던전 시작
exploreMode.RequestCommand(new ExploreStartDungeonCommand(1, "dungeon_2"));
```

## 이벤트 목록

- `ExplorePlayerJoinedEvent` - 플레이어 참가
- `ExploreDungeonStartedEvent` - 던전 시작
- `ExploreStageChangedEvent` - 층 이동
- `ExploreCombatStartedEvent` - 전투 시작
- `ExploreCombatActionEvent` - 공격 행동
- `ExploreCombatEndedEvent` - 전투 종료
- `ExploreRewardGrantedEvent` - 보상 획득
- `ExploreCharacterLevelUpEvent` - 레벨업
- `ExploreDungeonCompletedEvent` - 던전 완료
- `ExploreDungeonFailedEvent` - 던전 실패

## 확장 가능성

### 현재 단계 (MVP)
- [x] 자동 던전 진행
- [x] 자동 전투
- [x] 텍스트 로그 출력
- [x] 레벨업 시스템

### 향후 계획
- [ ] ScriptableObject 기반 던전/몬스터 정의
- [ ] 스킬 시스템 (AbilitySystem 통합)
- [ ] 장비 시스템
- [ ] 수동 전투 모드 (커맨드 입력)
- [ ] UI 추가 (HP 바, 인벤토리 등)
- [ ] 멀티플레이어 지원

## 스레드 안전성

- `ExploreHostState`: 모든 public 메서드 lock 보호
- `GameHostBase`: ConcurrentQueue로 커맨드/이벤트 처리
- Snapshot: 불변 객체로 안전한 데이터 전송
- Unity 메인 스레드: FlushEvents()로 이벤트 디스패치

## 성능

- **시뮬레이션**: 30 FPS (1/30초 = 33ms, Fixed Timestep)
- **스냅샷 생성**: 0.1초마다 (10 FPS)
- **던전 진행**: 2초마다 (AutoProgressInterval)
- **전투 턴**: 1초마다 (CombatTurnInterval)
- **상태 출력**: 5초마다 (StatusDisplayModule)

## 기술 스택

### Unity & C#
- Unity 2021.3 이상
- C# 9.0+ (record, pattern matching)
- .NET Standard 2.1

### 멀티스레딩
- System.Threading.Thread - Host 시뮬레이션 스레드
- System.Collections.Concurrent.ConcurrentQueue - 스레드 안전 큐
- lock (Monitor) - State 동기화
- Volatile.Read/Write - 스냅샷 캐싱

### 아키텍처 패턴
- Clean Architecture (Domain → Application → Presentation)
- Domain-Driven Design (Entity, Value Object, Aggregate)
- CQRS (Command Query Responsibility Segregation)
- Event Sourcing (일부 적용)
- Repository Pattern (State Container)
- Module Pattern (플러그인 아키텍처)

### Unity 특화
- MonoBehaviour - ExploreMode, TextLogModule
- SerializeField - Inspector 노출
- GameEventBus - Scene-scoped 이벤트
- Debug.Log - 색상 지원 텍스트 출력

## 파일 참조

### Shared Layer (재사용 인프라)
- [GameHostBase.cs](../../Shared/Application/GameHostBase.cs) - Host 기본 구조 (멀티스레드 시뮬레이션)
- [GameMode.cs](../../Shared/Presentation/GameMode/GameMode.cs) - Mode 기본 구조 (Module 생명주기 관리)
- [ModuleBase.cs](../../Shared/Presentation/GameMode/Module/ModuleBase.cs) - Module 기본 구조
- [GameHostTypes.cs](../../Shared/Domain/GameHostTypes.cs) - Command/Result/Event/Snapshot 기본 타입
- [GameEventBus.cs](../../Shared/Application/GameEvent/GameEventBus.cs) - 이벤트 버스
- [GameCommandOutcome.cs](../../Shared/Application/GameCommandOutcome.cs) - CQRS 결과 타입

### ExploreGame 구현 파일

#### Domain Layer
- [ExploreCharacterState.cs](Domain/ExploreCharacterState.cs)
- [ExploreMonsterState.cs](Domain/ExploreMonsterState.cs)
- [ExploreCombatState.cs](Domain/ExploreCombatState.cs)
- [ExploreDungeonState.cs](Domain/ExploreDungeonState.cs)
- [ExploreHostState.cs](Domain/ExploreHostState.cs)
- [ExploreStateSnapshot.cs](Domain/ExploreStateSnapshot.cs)

#### Application Layer
- [ExploreHost.cs](Application/Host/ExploreHost.cs)
- [ExploreModeHost.cs](Application/Host/ExploreModeHost.cs)
- [ExploreHostTypes.cs](Application/Host/ExploreHostTypes.cs)
- [ExploreDungeonModuleHost.cs](Application/Host/Modules/ExploreDungeonModuleHost.cs)
- [ExploreCombatModuleHost.cs](Application/Host/Modules/ExploreCombatModuleHost.cs)

#### Presentation Layer
- [ExploreMode.cs](Presentation/ExploreMode.cs)
- [ExploreViewModel.cs](Presentation/ViewModel/ExploreViewModel.cs)
- [ExploreHostEventRaisedEvent.cs](Presentation/Host/ExploreHostEventRaisedEvent.cs)
- [TextLogModule.cs](Presentation/Modules/TextLogModule.cs)
- [StatusDisplayModule.cs](Presentation/Modules/StatusDisplayModule.cs)

#### Data Layer
- [ExploreHostConfig.cs](Data/ExploreHostConfig.cs)
