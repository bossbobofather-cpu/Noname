# Noname - Unity Game Project

Noname는 Host-Client 아키텍처를 기반으로 한 Unity 게임 프로젝트입니다.  
게임 로직은 Host 스레드에서 시뮬레이션하고, Unity 메인 스레드는 결과를 표시하는 역할을 합니다.

## 주요 목표
- **안정적인 Host 시뮬레이션** (CQRS 기반 Command/Result/Event)
- **Clean Architecture** (Domain / Application / Presentation 분리)
- **플러그인 기반 공유 인프라** (GameHost, GameAbilitySystem)

## 프로젝트 구조

```
Assets/Scripts/
├── Features/
│   └── DefenseGame/
│       ├── Domain/           # 순수 C# 상태/데이터 모델
│       ├── Application/      # Host 로직 및 Command 처리
│       └── Presentation/     # Unity MonoBehaviour, View
└── Shared/
    ├── Application/          # 공통 Host/이벤트 인프라
    └── Presentation/         # GameMode, ModuleBase, UI

UpmPackages/
├── com.noname.gamehost.plugin/         # Host/CQRS 인프라
└── com.noname.gameabilitysystem.plugin/ # Ability System 플러그인
```

## 현재 포함된 게임
### DefenseGame
- 자동 전투 기반 방어형 게임
- Host가 전투/스폰/레벨업 처리
- Unity는 결과 이벤트와 스냅샷을 표시

## 플러그인
### GameHost Plugin
- Host 시뮬레이션 루프
- Command/Result/Event 큐
- Snapshot 생성 및 메인 스레드 전달

### GameAbilitySystem Plugin
- 순수 C# Domain 모델 + Unity Presentation 분리
- Gameplay Ability / Effect / Tag 기반 능력 시스템

## 실행 가이드 (개발용)
1. DefenseGame 관련 씬 또는 프리팹을 준비합니다.
2. `DefenseGameBootstrapper`와 `DefenseGameMode`를 배치합니다.
3. 플레이 모드에서 Host 시뮬레이션이 시작되고 결과가 출력됩니다.

## 참고
- Host 시뮬레이션은 별도 스레드에서 동작합니다.
- Unity API 호출은 반드시 메인 스레드에서 처리합니다.

## 라이선스
(추후 추가 예정)
