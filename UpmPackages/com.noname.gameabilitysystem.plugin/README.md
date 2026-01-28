# GameAbilitySystem Plugin

> Unity용 Gameplay Ability System - Clean Architecture 기반 확장 가능한 스킬/능력 시스템

[![Unity](https://img.shields.io/badge/Unity-6000.3.1f1-black)](https://unity.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-blue)](https://docs.microsoft.com/en-us/dotnet/csharp/)

## Overview

Unreal Engine의 Gameplay Ability System(GAS) 개념을 Unity에 최적화하여 구현한 플러그인입니다. Clean Architecture와 SOLID 원칙을 준수하며, **순수 C# Domain 레이어**와 **Unity 의존 Presentation 레이어**를 명확히 분리하여 멀티스레드 환경에서도 안전하게 동작합니다.

### Core Features

- ✅ **Clean Architecture**: Domain/Presentation 계층 분리로 Unity 독립적인 비즈니스 로직
- ✅ **Thread-Safe**: Host 환경에서 멀티스레드 안전하게 동작 (lock 기반 동기화)
- ✅ **Tag-Based System**: 계층적 Gameplay Tag로 능력/효과 활성화 제어
- ✅ **ScriptableObject Workflow**: 디자이너 친화적 데이터 주도 설계
- ✅ **Async Task Support**: AbilityTask로 비동기 능력 실행
- ✅ **Snapshot Pattern**: 불변 데이터 복사본으로 스레드 간 안전한 상태 전달

---

## Architecture

```
┌──────────────────────────────────────────────┐
│         Presentation Layer                   │
│      (Unity MonoBehaviour/SO)                │
│  ┌────────────────────────────────────────┐  │
│  │ AbilitySystemComponent (ViewModel)     │  │
│  └──────────────┬─────────────────────────┘  │
│                 │                             │
│  ┌──────────────▼─────────────────────────┐  │
│  │ Bridge (DomainConversionExtensions)    │  │
│  └──────────────┬─────────────────────────┘  │
└─────────────────┼──────────────────────────────┘
                  │
┌─────────────────▼──────────────────────────────┐
│          Domain Layer (Pure C#)                │
│  ┌────────────────────────────────────────┐   │
│  │ AbilitySystemModel (Thread-Safe)       │   │
│  │  - AttributeSetModel                   │   │
│  │  - GameplayTagContainerModel           │   │
│  │  - ActiveGameplayEffect[]              │   │
│  └────────────────────────────────────────┘   │
│                                                │
│  ✅ Host 환경 사용 가능 (멀티스레드 지원)       │
│  ✅ JSON 직렬화 가능                           │
│  ✅ Unity API 의존성 0%                        │
└────────────────────────────────────────────────┘
```

---

## Technical Highlights

### 1. Thread-Safe State Management

모든 public 메서드는 lock으로 보호되어 멀티스레드 환경에서 안전하게 동작합니다.

### 2. Tag-Based Activation Control

계층적 Gameplay Tag로 능력과 효과의 활성화 조건을 제어합니다.

### 3. Attribute Modifier System

Add, Multiply, Override 연산으로 속성 값을 유연하게 수정합니다.

### 4. Async Ability Execution

AbilityTask로 타겟팅, 애니메이션, 이벤트 대기 등을 비동기로 처리합니다.

---

## Key Components

| Component | Layer | Description |
|---|---|---|
| AbilitySystemModel | Domain | 스레드 안전 상태 관리 |
| AbilitySystemSnapshot | Domain | 불변 스냅샷 |
| GameplayEffectModel | Domain | 효과 데이터 모델 |
| AbilitySystemComponent | Presentation | Unity ViewModel |
| GameplayAbility | Presentation | 능력 실행 로직 |
| AbilityTask | Presentation | 비동기 Task |

---

## Performance

- **Zero Allocation**: Struct 기반 설계로 GC 최소화
- **Optimized Tag Lookup**: O(1) 해시 기반 검색
- **Lazy Initialization**: 필요 시점에만 초기화

---

## Documentation

- **API Reference**: DocFX로 생성된 API 문서
- **User Guide**: Wiki 참조
- **Architecture**: Domain/Presentation 폴더별 README 참조

---

## Author

**10+ Years Game Developer**
- Specialized in: Gameplay Systems, Multiplayer Architecture, Performance Optimization

---

## References

- [Unreal Engine - Gameplay Ability System](https://dev.epicgames.com/documentation/en-us/unreal-engine/gameplay-ability-system-for-unreal-engine)
- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
