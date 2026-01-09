# GameAbilitySystem 플러그인

## 개요
GameAbilitySystem은 능력(Ability), 태그(Tag), 이벤트(Event), 효과(Effect)를 기반으로 능력을 활성화하는 경량 시스템입니다.

## 핵심 타입
- `AbilitySystemComponent`: 능력 보유/활성화, 태그 관리, 효과 적용을 담당합니다.
- `GameplayAbility`: 능력 베이스 클래스입니다. `ActivateAbility`를 오버라이드해서 동작을 구현합니다.
- `GameplayAbilityDefinition`: 능력 타입과 Config 목록을 묶는 ScriptableObject입니다.
- `GameplayTagConfig`: 능력 태그, 활성화 필요/차단 태그를 정의합니다.
- `GameplayEffectConfig`: 효과 지속 시간과 부여 태그를 정의합니다.
- `GameplayEventTriggerConfig`: 이벤트 태그로 능력을 트리거합니다.
- `GameplayTagContainer` / `FGameplayTag`: 계층형 태그를 표현합니다.
- `GameplayTagRegistry`: 태그 목록을 에디터에서 관리합니다.
- `AttributeDefinition` / `AttributeSet`: 속성 정의용 타입이며 아직 ASC와 직접 연결되어 있지는 않습니다.

## 기본 사용 흐름
1. `GameplayTagRegistry` 에 태그를 정의합니다.
2. `GameplayAbility`를 상속한 능력 클래스를 작성합니다. (기본 생성자 필요)
3. `GameplayTagConfig`, `GameplayEffectConfig`, `GameplayEventTriggerConfig` 등을 생성합니다.
4. `GameplayAbilityDefinition`에서 Ability Type과 Config들을 연결합니다.
5. `AbilitySystemComponent`가 있는 오브젝트에 `IAbilitySystemProvider` 컴포넌트를 추가합니다.
6. `AbilitySystemComponent`의 `Startup Ability Definitions`에 능력 정의를 등록합니다.
7. 코드에서 능력 활성화나 이벤트 트리거를 호출합니다.

## 태그 규칙
- `A.B.C` 형태의 계층 태그를 사용합니다.
- 허용 문자: 영문, 숫자, `_`.
- 시작/끝에 `.` 금지, 연속된 `..` 금지.

## 이벤트 트리거
- `GameplayEventTriggerConfig.TriggerTag`를 설정하면 이벤트로 능력을 활성화할 수 있습니다.
- `HandleGameplayEvent`는 동일 태그뿐 아니라 부모 태그 일치도 허용합니다.

## 효과(Effect)
- `Instant`: 태그 적용 없이 즉시 처리됩니다. (현재는 태그 적용 없음)
- `Infinite`: 태그를 부여하고 유지합니다.
- `HasDuration`: 태그를 부여한 뒤 Duration 경과 시 자동 제거합니다.
- `Period` 값은 현재 로직에서 사용되지 않습니다.

## 예시 코드
```csharp
using UnityEngine;
using Noname.GameAbilitySystem;

public class DashAbility : GameplayAbility
{
    protected override void ActivateAbility(AbilityContext context)
    {
        // TODO: 대시 로직 구현
        EndAbility(context.Handle);
    }
}

public class PlayerAbilityProvider : MonoBehaviour, IAbilitySystemProvider
{
    [SerializeField] private AbilitySystemComponent _asc;

    public AbilitySystemComponent GetAbilitySystemComponent() => _asc;
}
```

```csharp
var eventData = new GameplayEventData
{
    EventTag = new FGameplayTag("Event.Hit"),
    Instigator = gameObject,
    Target = targetObject,
};

abilitySystemComponent.HandleGameplayEvent(eventData);
```

## 참고 사항
- `AbilitySystemComponent`는 `IAbilitySystemProvider`가 없으면 초기화되지 않습니다.
- `ActivationRequiredTags` / `ActivationBlockedTags`로 활성화 조건을 제어합니다.
- 태그 드롭다운은 `GameplayTagRegistry`가 존재할 때만 표시됩니다.
