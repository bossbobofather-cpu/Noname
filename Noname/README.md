# Noname - Unity Game Project

Unity 기반 게임 프로젝트입니다. Clean Architecture 패턴과 Host-Client 아키텍처를 사용하여 멀티스레드 환경을 지원합니다.

## 프로젝트 구조

```
Assets/Scripts/
├── Features/
│   ├── ExploreGame/          # 텍스트 로그라이크 던전 탐방 게임
│   │   ├── Domain/           # 도메인 모델 (순수 C#)
│   │   ├── Application/      # 비즈니스 로직 (Host)
│   │   ├── Presentation/     # Unity 프레젠테이션 계층
│   │   └── Data/             # 데이터 정의
│   │
│   └── MergeGame/            # 머지 게임
│       ├── Domain/
│       ├── Application/
│       ├── Presentation/
│       └── Data/
│
└── Shared/                    # 공통 시스템
    ├── Domain/
    ├── Application/          # GameHostBase, 이벤트 시스템
    └── Presentation/         # GameMode, ModuleBase, UI

UpmPackages/
└── com.noname.gameabilitysystem.plugin/  # Ability System Plugin
```

## 주요 기능

### 1. ExploreGame (탐험 게임)
- **텍스트 기반 로그라이크**: 자동 진행 던전 탐방 게임
- **ATB 전투 시스템**: Active Time Battle 턴제 전투
- **어빌리티 시스템**: 충전 기반 스킬 사용
- **SystemMessageUI**: 화면 하단 스택형 메시지 표시 (최대 30개)

**핵심 특징:**
- Host 시뮬레이션 스레드에서 게임 로직 실행 (60 TPS)
- 불변 Snapshot을 통한 스레드 안전한 상태 동기화
- 색상별 메시지 분류 (이벤트, 전투, 어빌리티, 보상 등)

### 2. Host-Client 아키텍처

모든 게임 로직은 별도 스레드에서 실행되며, Unity 메인 스레드와 안전하게 통신합니다.

```csharp
// Host (비즈니스 로직 스레드)
GameHostBase<Command, Result, Event, Snapshot>
  ├── Command Queue (커맨드 제출)
  ├── Event Queue (이벤트 발행)
  └── Snapshot Builder (상태 동기화)

// Client (Unity 메인 스레드)
GameMode
  ├── FlushEvents() (이벤트 소비)
  ├── Submit(Command) (커맨드 전송)
  └── ViewModel (Snapshot 기반)
```

**장점:**
- 멀티플레이어 준비 완료 (동일 Host 재사용)
- 디버깅 용이 (재현 가능한 시뮬레이션)
- Unity 독립적 비즈니스 로직

## 시작하기

### 필수 요구사항
- Unity 2021.3 이상
- UPM Package: GameAbilitySystem Plugin

### 실행 방법

1. **ExploreGame 실행**
   - Scene: `Assets/Scenes/ExploreGame/Game_Single.unity`
   - 자동으로 게임 시작 → 던전 입장 → 전투 진행
   - Console + SystemMessageUI로 진행 상황 확인

2. **MergeGame 실행**
   - Scene: `Assets/Scenes/MergeGame/Game_Single.unity`

## 아키텍처 패턴

### Clean Architecture
각 Feature는 3계층으로 구성됩니다:

1. **Domain**: 순수 C# 비즈니스 모델 (Unity 독립)
2. **Application**: Host 비즈니스 로직 (멀티스레드)
3. **Presentation**: Unity MonoBehaviour (메인 스레드)

### Module Pattern
GameMode는 여러 Module로 기능을 분리합니다:

```csharp
public class ExploreMode : GameMode
{
    // Modules
    - UIModule           // SystemMessageUI 출력
    - TextLogModule      // Console 로그 출력
    - StatusDisplayModule // 주기적 상태 출력
    - MapModule          // 맵 관리
    - RuleModule         // 게임 규칙
}
```

## 주요 시스템

### GameEventBus (이벤트 시스템)
Scene-scoped 이벤트 버스로 Module 간 통신을 담당합니다.

```csharp
// 발행
GameEventHub.Publish(new ExploreHostEventRaisedEvent(this, evt));

// 구독 (Module에서)
Mode.Subscribe<ExploreHostEventRaisedEvent>(OnHostEvent);
```

### SystemMessageBus (UI 메시지)
전역 이벤트 버스로 SystemMessageUI에 메시지를 전달합니다.

```csharp
SystemMessageBus.Publish("메시지", backgroundColor: Color.blue);
```

### AbilitySystem Plugin
GameplayAbility 기반 스킬 시스템을 제공합니다.

- **Domain Models**: 순수 C# 어빌리티 데이터 모델
- **Presentation**: Unity ScriptableObject (디자이너 툴)
- **Runtime**: Host에서 사용 가능한 순수 C# 모델

자세한 내용은 [AbilitySystem README](UpmPackages/com.noname.gameabilitysystem.plugin/README.md)를 참조하세요.

## 개발 가이드

### 새 게임 추가하기

1. **폴더 구조 생성**
   ```
   Assets/Scripts/Features/NewGame/
   ├── Domain/         # 상태 클래스 (State, Snapshot)
   ├── Application/    # Host, ModuleHost
   ├── Presentation/   # GameMode, Modules, ViewModel
   └── Data/           # Config, ScriptableObject
   ```

2. **Host 생성**
   ```csharp
   public class NewGameHost : GameHostBase<NewGameCommand, NewGameCommandResult, NewGameHostEvent, NewGameSnapshot>
   {
       // 비즈니스 로직 구현
   }
   ```

3. **GameMode 생성**
   ```csharp
   public class NewGameMode : GameMode
   {
       private INewGameHost _host;

       protected override void OnInitialize()
       {
           _host = new NewGameHost(config, randomSource);
           _host.StartSimulation();
       }
   }
   ```

### 테스트 환경 구성

1. **GameEventBus 초기화**
   ```csharp
   [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
   private static void Initialize()
   {
       GameEventHub.SetActiveScene(SceneManager.GetActiveScene());
   }
   ```

2. **씬 설정**
   - UIRoot Prefab 배치
   - GameMode 컴포넌트 추가
   - Module 컴포넌트 추가

## 디버깅

### Host 시뮬레이션 확인
```csharp
Debug.Log($"Host Running: {_host.IsRunning}");
Debug.Log($"Tick: {_host.Tick}");
```

### 이벤트 추적
```csharp
_host.EventRaised += evt => Debug.Log($"Event: {evt.GetType().Name}");
```

### Snapshot 확인
```csharp
var snapshot = _viewModel.GetSnapshot();
Debug.Log($"Phase: {snapshot.SessionPhase}");
```

## 라이선스

(라이선스 정보 추가 예정)

## 기여

(기여 가이드 추가 예정)
