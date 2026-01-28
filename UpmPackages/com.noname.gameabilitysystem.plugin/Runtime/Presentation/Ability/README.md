# Ability Layer

어빌리티의 실행 로직을 담당하는 계층입니다. GameplayAbility 추상 클래스와 관련 인프라를 제공합니다.

## 구조

```
Ability/
├── GameplayAbility.cs            # 어빌리티 추상 베이스 클래스
├── GameplayAbilitySpec.cs        # 어빌리티 스펙 (부여된 어빌리티 정보)
├── GameplayAbilityInstance.cs    # 실행 중인 어빌리티 인스턴스
├── AbilityContext.cs             # 어빌리티 실행 컨텍스트
└── AbilitySystemComponent.cs     # (Presentation으로 이동됨)
```

## 핵심 개념

### GameplayAbility (실행 로직)

어빌리티의 동작을 정의하는 추상 클래스입니다. 모든 커스텀 어빌리티는 이 클래스를 상속받습니다.

```csharp
public abstract class GameplayAbility
{
    protected AbilitySystemComponent ASC { get; }  // 소유자의 Ability System
    protected IAbilityTaskOwner TaskOwner { get; }  // Task 소유자
    public IReadOnlyList<GameplayConfig> Configs { get; }  // 연결된 Config 목록

    // 생명주기 메서드
    protected virtual void OnInit() { }
    protected virtual void PreActivate(AbilityContext context) { }
    protected virtual void ActivateAbility(AbilityContext context) { }
    public virtual void CancelAbility(FGameplayAbilitySpecHandle handle) { }
    public virtual void EndAbility(FGameplayAbilitySpecHandle handle) { }

    // 조건 검사
    public virtual bool CanActivateAbility() { return true; }

    // Config 조회
    public bool TryGetConfig<T>(out T config) where T : GameplayConfig { }
    public bool TryGetConfigs<T>(out List<T> configs) where T : GameplayConfig { }
}
```

### 생명주기

```
1. GiveAbility()
   └→ OnInit()

2. TryActivateAbility()
   ├→ CanActivateAbility()  (조건 검사)
   ├→ PreActivate()         (준비 단계)
   └→ ActivateAbility()     (실제 로직)

3. EndAbility() / CancelAbility()
   └→ 종료 처리
```

## 기본 어빌리티 작성

### 간단한 공격 어빌리티

```csharp
using Noname.GameAbilitySystem;
using UnityEngine;

public class BasicAttackAbility : GameplayAbility
{
    protected override void ActivateAbility(AbilityContext context)
    {
        // 1. 타겟 획득
        var targetData = context.TargetData;
        if (targetData == null || targetData.TargetActor == null)
        {
            Debug.LogWarning("No target for basic attack");
            return;
        }

        // 2. 타겟의 AbilitySystemComponent 획득
        var targetASC = targetData.TargetActor.GetComponent<AbilitySystemComponent>();
        if (targetASC == null)
        {
            Debug.LogWarning("Target has no AbilitySystemComponent");
            return;
        }

        // 3. 효과 적용 (Config에서 자동 적용됨)
        // GameplayEffectConfig가 AbilityDefinition의 Configs에 포함되어 있으면
        // AbilitySystemComponent가 자동으로 적용

        Debug.Log($"Basic attack activated on {targetData.TargetActor.name}");
    }
}
```

### 조건부 어빌리티

```csharp
public class FireballAbility : GameplayAbility
{
    private int _manaCost = 50;

    public override bool CanActivateAbility()
    {
        // 마나 체크
        if (!ASC.Attributes.TryGet(AttributeId.Mana, out var mana))
        {
            return false;
        }

        if (mana.CurrentValue < _manaCost)
        {
            Debug.Log("Not enough mana");
            return false;
        }

        return base.CanActivateAbility();
    }

    protected override void ActivateAbility(AbilityContext context)
    {
        // 마나 소모
        if (ASC.Attributes.TryGet(AttributeId.Mana, out var mana))
        {
            mana.CurrentValue -= _manaCost;
        }

        // 파이어볼 발사
        LaunchFireball(context.TargetData);
    }

    private void LaunchFireball(AbilityTargetData targetData)
    {
        // 투사체 생성 로직
        Debug.Log("Fireball launched!");
    }
}
```

### Config 기반 어빌리티

```csharp
public class ConfigurableAbility : GameplayAbility
{
    private GameplayEffectConfig _damageEffect;
    private GameplayTagConfig _tagConfig;

    protected override void OnInit()
    {
        base.OnInit();

        // Config 로딩
        TryGetConfig(out _damageEffect);
        TryGetConfig(out _tagConfig);
    }

    public override bool CanActivateAbility()
    {
        // TagConfig 기반 조건 검사
        if (_tagConfig != null)
        {
            if (!ASC.OwnedTags.HasAll(_tagConfig.ActivationRequiredTags))
            {
                return false;
            }

            if (ASC.OwnedTags.HasAny(_tagConfig.ActivationBlockedTags))
            {
                return false;
            }
        }

        return base.CanActivateAbility();
    }

    protected override void ActivateAbility(AbilityContext context)
    {
        // EffectConfig 기반 효과 적용
        if (_damageEffect != null && context.TargetData != null)
        {
            var targetASC = context.TargetData.TargetActor.GetComponent<AbilitySystemComponent>();
            if (targetASC != null)
            {
                var effectContext = new GameplayEffectContext(ASC, targetASC, context.EventData);
                targetASC.ApplyGameplayEffect(_damageEffect, effectContext);
            }
        }
    }
}
```

## AbilityContext

어빌리티 실행에 필요한 컨텍스트 정보를 담습니다.

```csharp
public readonly struct AbilityContext
{
    public FGameplayAbilitySpecHandle Handle { get; }      // 어빌리티 핸들
    public GameplayEventData EventData { get; }            // 트리거 이벤트 데이터
    public AbilityTargetData TargetData { get; }           // 타겟 데이터

    // 타겟만 변경한 새 컨텍스트 생성
    public AbilityContext WithTargetData(AbilityTargetData targetData)
    {
        return new AbilityContext(Handle, EventData, targetData);
    }
}
```

**사용 예:**
```csharp
protected override void ActivateAbility(AbilityContext context)
{
    // 핸들 조회
    var handle = context.Handle;

    // 이벤트 데이터 조회
    if (context.EventData.EventTag.Equals(new FGameplayTag("Event.Damaged")))
    {
        Debug.Log("Triggered by damage event");
    }

    // 타겟 데이터 조회
    var target = context.TargetData?.TargetActor;
}
```

## GameplayAbilitySpec

부여된 어빌리티의 정보를 담는 클래스입니다.

```csharp
public sealed class GameplayAbilitySpec
{
    public Type AbilityType;                            // 어빌리티 클래스 타입
    public string AbilityName;                          // 에디터 표시용 이름
    public IReadOnlyList<GameplayConfig> Configs;       // 연결된 Config 목록
    public int Level;                                   // 어빌리티 레벨
    public int ActiveCount;                             // 활성 인스턴스 개수
    public FGameplayAbilitySpecHandle Handle;           // 고유 핸들

    // Config 조회
    public bool TryGetConfig<T>(out T config) where T : GameplayConfig { }
    public bool TryGetConfigs<T>(out List<T> configs) where T : GameplayConfig { }
}
```

**내부 사용:**
```csharp
// AbilitySystemComponent 내부에서 관리
private readonly List<GameplayAbilitySpec> _abilities = new();

// 부여 시 생성
var spec = new GameplayAbilitySpec
{
    AbilityType = typeof(FireballAbility),
    AbilityName = "Fireball",
    Configs = configs,
    Level = 1,
    ActiveCount = 0,
    Handle = new FGameplayAbilitySpecHandle { Id = _nextHandleId++ }
};

_abilities.Add(spec);
```

## GameplayAbilityInstance

실행 중인 어빌리티 인스턴스를 관리합니다.

```csharp
public sealed class GameplayAbilityInstance : IAbilityTaskOwner
{
    public AbilitySystemComponent ASC { get; }
    public AbilityContext Context { get; private set; }
    public FGameplayAbilitySpecHandle Handle { get; }

    public void Activate()           // 어빌리티 활성화
    public void End()                // 어빌리티 종료
    public void Cancel()             // 어빌리티 취소
    public void UpdateContext(AbilityContext context)  // 컨텍스트 갱신

    // Coroutine 지원
    public Coroutine StartCoroutine(IEnumerator routine)
    public void StopCoroutine(Coroutine routine)

    // Task 관리
    public void RegisterTask(AbilityTask task)
    public void UnregisterTask(AbilityTask task)
}
```

## 고급 패턴

### 타겟팅이 필요한 어빌리티

```csharp
public class TargetedAttackAbility : GameplayAbility
{
    protected override void ActivateAbility(AbilityContext context)
    {
        // 타겟 데이터가 없으면 타겟팅 시작
        if (context.TargetData == null)
        {
            if (TryGetConfig<GameplayTargetConfig>(out var targetConfig))
            {
                // Task를 사용해 타겟 획득
                var task = AbilityTask_WaitTargetData.Create(this, targetConfig);
                task.ValidData += (targetData) =>
                {
                    // 타겟 획득 후 다시 활성화
                    var newContext = context.WithTargetData(targetData);
                    TaskOwner.UpdateContext(newContext);
                    ActivateAbility(newContext);
                };
                task.Activate();
            }
        }
        else
        {
            // 타겟이 있으면 공격 실행
            PerformAttack(context.TargetData);
        }
    }

    private void PerformAttack(AbilityTargetData targetData)
    {
        Debug.Log($"Attacking {targetData.TargetActor.name}");
    }
}
```

### 이벤트 트리거 어빌리티

```csharp
public class CounterAttackAbility : GameplayAbility
{
    protected override void OnInit()
    {
        base.OnInit();

        // EventTriggerConfig 확인
        if (TryGetConfig<GameplayEventTriggerConfig>(out var trigger))
        {
            Debug.Log($"Will activate on event: {trigger.TriggerTag.Value}");
        }
    }

    protected override void ActivateAbility(AbilityContext context)
    {
        // 이벤트 데이터에서 공격자 정보 추출
        var attacker = context.EventData.Instigator;
        if (attacker != null)
        {
            Debug.Log($"Counter-attacking {attacker.name}");
            // 반격 로직
        }
    }
}
```

### 지속형 어빌리티

```csharp
public class ShieldAbility : GameplayAbility
{
    private GameplayEffectConfig _shieldEffect;

    protected override void OnInit()
    {
        base.OnInit();
        TryGetConfig(out _shieldEffect);
    }

    protected override void ActivateAbility(AbilityContext context)
    {
        // Infinite Duration 효과 적용
        if (_shieldEffect != null)
        {
            ASC.ApplyGameplayEffect(_shieldEffect);
        }

        // 일정 시간 후 자동 종료
        TaskOwner.StartCoroutine(EndAfterDelay(5f));
    }

    private IEnumerator EndAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 효과 제거
        if (_shieldEffect != null)
        {
            ASC.RemoveGameplayEffect(_shieldEffect);
        }

        // 어빌리티 종료
        EndAbility(TaskOwner.Handle);
    }

    public override void CancelAbility(FGameplayAbilitySpecHandle handle)
    {
        // 효과 제거
        if (_shieldEffect != null)
        {
            ASC.RemoveGameplayEffect(_shieldEffect);
        }
    }
}
```

## 디버깅 팁

### 로그 출력
```csharp
protected override void PreActivate(AbilityContext context)
{
    Debug.Log($"[{GetType().Name}] PreActivate - Handle: {context.Handle.Id}");
}

protected override void ActivateAbility(AbilityContext context)
{
    Debug.Log($"[{GetType().Name}] ActivateAbility");

    // 타겟 확인
    if (context.TargetData != null)
    {
        Debug.Log($"  Target: {context.TargetData.TargetActor.name}");
    }

    // 이벤트 확인
    if (context.EventData.EventTag.IsValid)
    {
        Debug.Log($"  Event: {context.EventData.EventTag.Value}");
    }
}
```

### Gizmos 그리기
```csharp
public class RangedAttackAbility : GameplayAbility
{
    [SerializeField] private float _range = 10f;

    protected override void ActivateAbility(AbilityContext context)
    {
        // 범위 내 타겟 검색
        var targets = FindTargetsInRange(ASC.transform.position, _range);
        Debug.Log($"Found {targets.Count} targets in range");
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (ASC != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(ASC.transform.position, _range);
        }
    }
#endif
}
```

## 베스트 프랙티스

1. **조건 검사는 CanActivateAbility에서**
   ```csharp
   public override bool CanActivateAbility()
   {
       // 리소스, 태그, 상태 체크
       return HasEnoughMana() && !IsStunned();
   }
   ```

2. **초기화는 OnInit에서**
   ```csharp
   protected override void OnInit()
   {
       base.OnInit();
       // Config 로딩, 캐싱
       TryGetConfig(out _effectConfig);
   }
   ```

3. **정리는 EndAbility/CancelAbility에서**
   ```csharp
   public override void EndAbility(FGameplayAbilitySpecHandle handle)
   {
       // 임시 효과 제거, 리소스 해제
       CleanupResources();
   }
   ```

4. **Config 기반 설계**
   - 하드코딩 대신 Config에서 값 읽기
   - 디자이너가 조정 가능하도록

5. **null 체크 필수**
   ```csharp
   if (context.TargetData?.TargetActor != null)
   {
       // 안전하게 사용
   }
   ```

## 다음 단계

- [Domain 폴더](../Domain/README.md) - 순수 C# 모델
- [Presentation 폴더](../Presentation/README.md) - Unity 컴포넌트와 ScriptableObject
