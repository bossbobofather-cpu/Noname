# GameAbilitySystem Plugin - API 문서

## 개요

**GameAbilitySystem Plugin**은 언리얼의 Gameplay Ability System(GAS)을 Unity 환경에 맞춰 재구성한 플러그인입니다. Clean Architecture 원칙을 따르며, Host 시뮬레이션 환경에서도 사용 가능한 구조를 지향합니다.

### 핵심 특징

- **Clean Architecture**: Domain/Presentation 분리, Domain은 Unity 의존 없음
- **Thread-Safe**: lock 기반 동기화로 별도 스레드 실행 가능
- **고성능 지향**: 구조체 기반 태그와 O(1) 조회
- **확장성**: 태그 기반 활성화 + 커스텀 계산기/태스크
- **실전 지향**: 실제 게임 개발 경험에서 검증된 패턴

---

## 아키텍처 레이어

```
Presentation 레이어 (Unity)
- AbilitySystemComponent
- GameplayAbility
- GameplayEffectConfig

Bridge 레이어
- DomainConversionExtensions

Domain 레이어 (Pure C#, Unity-Free)
- AbilitySystemModel
- AttributeSetModel
- GameplayTagContainerModel
```

---

## 핵심 구성 요소

### Domain 레이어 (`Noname.GameAbilitySystem.Domain`)

Domain 레이어는 **Unity 의존성이 전혀 없는 순수 C# 로직**으로 구성됩니다. 모든 클래스는 스레드 안전하게 설계되어 Host 환경에서도 실행할 수 있습니다.

#### AbilitySystemModel
속성/태그/효과 상태를 관리하는 스레드 안전 모델입니다.

**주요 메서드:**
- `Set(AttributeId, float)` - 속성 값 설정
- `AddTag(FGameplayTagModel)` - 태그 추가
- `AddActiveEffect(GameplayEffectModel, float)` - 효과 적용
- `BuildSnapshot()` - 렌더링용 스냅샷 생성

#### AttributeSetModel
체력/마나/공격력 등 속성값을 관리하며, 최소/최대 클램핑을 제공합니다.

**주요 메서드:**
- `SetAttribute(AttributeId, float, float, float)` - 속성 초기화
- `TryGet(AttributeId, out AttributeValueModel)` - 값 조회
- `Modify(AttributeId, float, ModifierOperationType)` - 수정자 적용

#### GameplayTagContainerModel
해시 기반 컨테이너로 O(1) 태그 조회를 지원합니다.

**주요 메서드:**
- `HasTag(FGameplayTagModel)` - 태그 존재 여부
- `HasAll(IEnumerable<FGameplayTagModel>)` - 다중 요구 태그 체크
- `HasAny(IEnumerable<FGameplayTagModel>)` - 하나라도 만족하는지 체크

---

### Presentation 레이어 (`Noname.GameAbilitySystem.Presentation`)

Unity 전용 컴포넌트 및 ScriptableObject 기반 설정을 포함합니다.

#### AbilitySystemComponent
능력/효과/태그를 관리하는 Unity 컴포넌트입니다.

**주요 메서드:**
- `GiveAbility(GameplayAbilityDefinition)` - 능력 부여
- `TryActivateAbilityByType<T>()` - 타입 기반 활성화
- `ApplyGameplayEffect(GameplayEffectConfig)` - 효과 적용
- `HandleGameplayEvent(GameplayEventData)` - 이벤트 처리

#### GameplayAbility
비동기 태스크 기반 능력 구현을 위한 베이스 클래스입니다.

**주요 메서드:**
- `ActivateAbility(AbilityContext)` - 활성화 로직
- `CanActivateAbility()` - 활성화 가능 여부
- `EndAbility(FGameplayAbilitySpecHandle)` - 정상 종료
- `CancelAbility(FGameplayAbilitySpecHandle)` - 강제 취소

---

## 성능 특성

| 항목 | 복잡도 | 설명 |
|------|--------|------|
| 태그 조회 | O(1) | 해시 기반 컨테이너 |
| 속성 수정 | O(1) | 딕셔너리 조회 |
| 효과 적용 | O(n) | n = 수정자 개수 |
| 스냅샷 생성 | O(m) | m = 전체 상태 크기 |

### 메모리 효율
- **구조체 태그**: FGameplayTag는 문자열 참조 + 해시
- **GC 최소화**: 런타임 중 불필요한 할당 억제
- **해시 캐싱**: StringToHash 결과 보관

---

## 시작하기

### 간단 예시

```csharp
using Noname.GameAbilitySystem.Presentation;
using UnityEngine;

public class MyFireballAbility : GameplayAbility
{
    protected override void ActivateAbility(AbilityContext context)
    {
        // 1. 타겟 획득
        var targetTask = AbilityTask_WaitTargetData.Create(this, targetConfig);
        targetTask.ValidData += OnTargetAcquired;
        targetTask.Activate();
    }

    private void OnTargetAcquired(AbilityTargetData data)
    {
        // 2. 효과 적용 (Config 기반)
        EndAbility(TaskOwner.Handle);
    }

    public override bool CanActivateAbility()
    {
        // 마나 체크
        return ASC.Attributes.TryGet(AttributeId.Mana, out var mana)
            && mana.CurrentValue >= 50f;
    }
}
```

---

## API 레퍼런스

- **[Domain 네임스페이스](xref:Noname.GameAbilitySystem.Domain)**
- **[Presentation 네임스페이스](xref:Noname.GameAbilitySystem.Presentation)**

---

## 기술 사양

- **Unity 버전**: 6000.3.1f1 이상
- **.NET Target**: .NET Standard 2.1
- **Threading**: Domain 레이어 스레드 안전 보장
- **Serialization**: JSON 기반 스냅샷 호환
- **Dependencies**: 외부 의존 없음

---

## 정리

이 플러그인은 다음을 중점적으로 보여줍니다:

- Clean Architecture 기반 분리
- Host/Client 분리 구조 대응
- 성능을 고려한 태그/속성 구조
- 확장 가능한 태스크/이펙트 설계

문의사항은 저장소를 참고해주세요.
