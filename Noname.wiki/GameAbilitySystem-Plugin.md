# GameAbilitySystem Plugin - Technical Guide

## 개요

본 플러그인은 **Unreal Engine의 Gameplay Ability System(GAS)**을 Unity 환경에 맞게 재설계한 것으로, Clean Architecture와 SOLID 원칙을 준수하여 확장 가능하고 유지보수 가능한 능력 시스템을 제공합니다.

---

## 아키텍처 설계

### Clean Architecture: Domain/Presentation 분리

```
┌─────────────────────────────────────────────────────┐
│           Presentation Layer                        │
│        (Unity MonoBehaviour/ScriptableObject)       │
│  ┌───────────────────────────────────────────────┐  │
│  │ AbilitySystemComponent (ViewModel)            │  │
│  │  - GiveAbility / TryActivateAbility           │  │
│  │  - ApplyGameplayEffect                        │  │
│  │  - HandleGameplayEvent                        │  │
│  └──────────────────┬────────────────────────────┘  │
│                     │                                │
│  ┌──────────────────▼────────────────────────────┐  │
│  │ Bridge Layer                                  │  │
│  │  - GameplayEffectConfig.ToDomain()            │  │
│  │  - FGameplayTag.ToDomain()                    │  │
│  └──────────────────┬────────────────────────────┘  │
└─────────────────────┼──────────────────────────────┘
                      │
┌─────────────────────▼──────────────────────────────┐
│              Domain Layer                          │
│           (Pure C#, Unity-Free)                    │
│  ┌───────────────────────────────────────────────┐ │
│  │ AbilitySystemModel                            │ │
│  │  - lock 기반 Thread-Safe                      │ │
│  │  - AttributeSetModel (속성 관리)              │ │
│  │  - GameplayTagContainerModel (태그 관리)      │ │
│  │  - ActiveGameplayEffect[] (효과 관리)         │ │
│  └───────────────────────────────────────────────┘ │
│                                                     │
│  ✅ Host 환경 사용 가능 (별도 스레드 실행)           │
│  ✅ JSON 직렬화 가능 (네트워크/저장 연동)            │
│  ✅ Unity API 의존성 0% (.NET Standard 2.1)        │
└─────────────────────────────────────────────────────┘
```

### 의존성 규칙

```
Presentation → Domain  ✅ (올바른 방향)
Domain → Presentation  ❌ (네임스페이스로 컴파일 에러 방지)
```

**네임스페이스:**
- `Noname.GameAbilitySystem.Domain` - 순수 C#
- `Noname.GameAbilitySystem.Presentation` - Unity 의존

---

## 핵심 컴포넌트

### 1. AbilitySystemModel (Domain)

**Thread-Safe 상태 관리**

```csharp
namespace Noname.GameAbilitySystem.Domain
{
    public sealed class AbilitySystemModel
    {
        private readonly object _modelLock = new();
        private readonly AttributeSetModel _attributes;
        private readonly GameplayTagContainerModel _tags;
        private readonly List<ActiveGameplayEffect> _activeEffects;

        // 모든 public 메서드는 lock으로 보호
        public void Set(AttributeId id, float value)
        {
            lock (_modelLock)
            {
                _attributes.Set(id, value);
            }
        }

        // 불변 스냅샷 생성 (Snapshot Pattern)
        public AbilitySystemSnapshot BuildSnapshot()
        {
            lock (_modelLock)
            {
                return new AbilitySystemSnapshot(
                    CopyAttributes(),
                    CopyTags(),
                    CopySkills(),
                    CopyActiveEffects()
                );
            }
        }
    }
}
```

**특징:**
- ✅ 멀티스레드 안전 (모든 public 메서드 lock)
- ✅ Unity API 의존성 0%
- ✅ Host 환경에서 별도 스레드 실행 가능
- ✅ JSON 직렬화 지원

---

### 2. AbilitySystemComponent (Presentation)

**Unity ViewModel**

```csharp
namespace Noname.GameAbilitySystem.Presentation
{
    public sealed class AbilitySystemComponent : MonoBehaviour
    {
        private AbilitySystemModel _model;  // Domain 모델

        public void ApplyGameplayEffect(GameplayEffectConfig config)
        {
            // ScriptableObject → Domain Model 변환
            var model = config.ToDomain();

            // Domain 모델에 적용
            _model.AddActiveEffect(model, Time.time + config.Duration);
        }

        public bool TryActivateAbility<T>() where T : GameplayAbility
        {
            // Tag 조건 체크
            if (!CheckActivationTags()) return false;

            // Ability 인스턴스 생성 및 실행
            var ability = CreateAbilityInstance<T>();
            ability.ActivateAbility(context);

            return true;
        }
    }
}
```

**역할:**
- Unity와 Domain 레이어 브리지
- ScriptableObject 기반 워크플로우 제공
- 능력 부여/활성화/종료 관리

---

### 3. GameplayAbility (Presentation)

**능력 실행 로직 베이스 클래스**

```csharp
public abstract class GameplayAbility
{
    protected AbilitySystemComponent ASC { get; }

    // 생명주기
    protected virtual void OnInit() { }
    protected virtual void PreActivate(AbilityContext context) { }
    protected abstract void ActivateAbility(AbilityContext context);
    public virtual void EndAbility(FGameplayAbilitySpecHandle handle) { }
    public virtual void CancelAbility(FGameplayAbilitySpecHandle handle) { }

    // 조건 검사
    public virtual bool CanActivateAbility() { return true; }
}
```

**구현 예제:**

```csharp
public class FireballAbility : GameplayAbility
{
    protected override void ActivateAbility(AbilityContext context)
    {
        // 1. 타겟 획득 (비동기)
        var targetTask = AbilityTask_WaitTargetData.Create(this, targetConfig);
        targetTask.ValidData += OnTargetAcquired;
        targetTask.Activate();
    }

    private void OnTargetAcquired(AbilityTargetData targetData)
    {
        // 2. 타겟에 효과 적용 (Config에서 자동)
        var targetASC = targetData.TargetActor.GetComponent<AbilitySystemComponent>();
        // GameplayEffectConfig가 자동으로 Domain 모델로 변환되어 적용됨

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

## 핵심 시스템

### 1. Tag-Based Activation

**계층적 Gameplay Tag**

```
Ability.Attack.Melee
Ability.Attack.Ranged
Ability.Magic.Fire
Status.Burning
Status.Frozen
Player.Alive
```

**활성화 조건 제어:**

```csharp
[GameplayTagConfig]
ActivationRequiredTags: ["Player.Alive", "Player.HasMana"]
ActivationBlockedTags: ["Status.Stunned", "Status.Silenced"]

// 런타임 체크
if (!OwnedTags.HasAll(RequiredTags)) return false;
if (OwnedTags.HasAny(BlockedTags)) return false;
```

**효과 적용 조건:**

```csharp
[GameplayEffectConfig]
RequiredTags: ["Enemy.Undead"]  // 언데드에게만 적용
BlockedTags: ["Buff.HolyImmune"]  // 성스러운 면역이 있으면 차단
```

---

### 2. Attribute Modifier System

**수정자 타입:**

```csharp
public enum ModifierOperationType
{
    Add,         // CurrentValue + Value
    AddPercent,  // CurrentValue * (1 + Value/100)
    Multiply,    // CurrentValue * Value
    Override     // Value로 덮어쓰기
}
```

**효과 정의 (Domain 모델):**

```csharp
var damageEffect = new GameplayEffectModel
{
    EffectId = "BasicAttack",
    DurationType = EffectDurationType.Instant,
    ModifierGroups = new List<GameplayModifierGroup>
    {
        new GameplayModifierGroup
        {
            Modifiers = new List<GameplayModifier>
            {
                new GameplayModifier("Health", ModifierOperationType.Add, -50f)
            }
        }
    }
};
```

**지속 타입:**

```csharp
// Instant: 즉시 적용 후 사라짐
DurationType = EffectDurationType.Instant

// HasDuration: 일정 시간 후 만료
DurationType = EffectDurationType.HasDuration
Duration = 10f  // 10초 지속

// Infinite: 수동 제거 전까지 유지
DurationType = EffectDurationType.Infinite
```

---

### 3. Async Ability Task

**AbilityTask 종류:**

| Task | 용도 |
|---|---|
| `AbilityTask_WaitTargetData` | 타겟 획득 대기 |
| `AbilityTask_PlayMontageAndWait` | 애니메이션 재생 대기 |
| `AbilityTask_WaitGameplayEvent` | 이벤트 발생 대기 |
| `AbilityTask_WaitInputPress` | 입력 대기 |

**사용 예제:**

```csharp
public class ChargedAttackAbility : GameplayAbility
{
    protected override void ActivateAbility(AbilityContext context)
    {
        // 1. 애니메이션 재생
        var montageTask = AbilityTask_PlayMontageAndWait.Create(this, chargeMontage);
        montageTask.Completed += OnChargeComplete;
        montageTask.Activate();
    }

    private void OnChargeComplete()
    {
        // 2. 타겟 획득
        var targetTask = AbilityTask_WaitTargetData.Create(this, targetConfig);
        targetTask.ValidData += (data) => {
            // 3. 공격 실행
            ExecuteAttack(data);
            EndAbility(TaskOwner.Handle);
        };
        targetTask.Activate();
    }
}
```

---

## 성능 최적화

### 1. Zero Allocation

```csharp
// Struct 기반 태그/이벤트
public struct FGameplayTagModel  // 16 bytes (string ref + int hash)
{
    private string _value;
    private int _hash;
}

public struct GameplayEventData  // 24 bytes (struct + object ref)
{
    public FGameplayTag EventTag;
    public object Payload;
}
```

### 2. String Hash Caching

```csharp
public struct FGameplayTag
{
    private int _hash;

    public int Hash
    {
        get
        {
            if (_hash == 0 && !string.IsNullOrEmpty(_value))
                _hash = Animator.StringToHash(_value);  // Unity 해시 함수 활용
            return _hash;
        }
    }
}
```

### 3. O(1) Tag Lookup

```csharp
public sealed class GameplayTagContainerModel
{
    private readonly HashSet<int> _explicitTags;  // O(1) Contains
    private readonly HashSet<int> _expandedTags;  // 부모 태그 포함

    public bool HasTag(FGameplayTagModel tag)
    {
        return _expandedTags.Contains(tag.Hash);  // O(1)
    }
}
```

---

## 워크플로우

### ScriptableObject 기반 워크플로우

```
1. [디자이너] GameplayTagRegistry에 태그 정의
   └─> "Ability.Attack.Fireball"

2. [디자이너] GameplayEffectConfig 생성
   └─> DamageEffect, ManaCostEffect

3. [디자이너] GameplayAbilityDefinition 생성
   └─> Ability Type: FireballAbility
   └─> Configs: [TagConfig, EffectConfig, TargetConfig]

4. [프로그래머] FireballAbility 클래스 구현
   └─> ActivateAbility() 로직 작성

5. [디자이너] GameObject에 AbilityDefinition 할당
   └─> Startup Abilities에 추가

6. [런타임] 자동 부여 및 활성화
```

---

## 테스트

### Unit Test (Domain Layer)

```csharp
[Test]
public void TestAttributeModification()
{
    // Given
    var model = new AbilitySystemModel();
    model.InitializeAttribute(AttributeId.Health, 100f);

    // When
    model.Add(AttributeId.Health, -30f);

    // Then
    Assert.AreEqual(70f, model.Get(AttributeId.Health));
}

[Test]
public void TestThreadSafety()
{
    var model = new AbilitySystemModel();

    // 1000개 스레드에서 동시 접근
    Parallel.For(0, 1000, i =>
    {
        model.Set(AttributeId.Health, i);
    });

    // Race condition 없음 ✅
}
```

---

## 확장 포인트

### 1. Custom GameplayEffectCalculator

```csharp
[CreateAssetMenu(menuName = "GameAbilitySystem/Calculator/Critical")]
public class CriticalDamageCalculator : GameplayEffectCalculator
{
    [SerializeField] private float _critChance = 0.2f;
    [SerializeField] private float _critMultiplier = 2f;

    public override float EvaluateMagnitude(
        GameplayEffectConfig config,
        AttributeModifier modifier,
        GameplayEffectContext context)
    {
        var isCrit = Random.value < _critChance;
        return isCrit ? modifier.Magnitude * _critMultiplier : modifier.Magnitude;
    }
}
```

### 2. Custom AbilityTask

```csharp
public class AbilityTask_WaitDelay : AbilityTask
{
    private float _duration;

    public static AbilityTask_WaitDelay Create(GameplayAbility ability, float duration)
    {
        var task = CreateTask<AbilityTask_WaitDelay>(ability);
        task._duration = duration;
        return task;
    }

    public override void Activate()
    {
        TaskOwner.StartCoroutine(WaitCoroutine());
    }

    private IEnumerator WaitCoroutine()
    {
        yield return new WaitForSeconds(_duration);
        OnCompleted?.Invoke();
        EndTask();
    }
}
```

---

## DocFX API 문서

### XML 주석 규칙

```csharp
/// <summary>
/// 능력을 활성화합니다.
/// </summary>
/// <param name="context">활성화 컨텍스트 (핸들, 이벤트 데이터, 타겟 데이터)</param>
/// <returns>활성화 성공 여부</returns>
/// <exception cref="ArgumentNullException">context가 null인 경우</exception>
/// <example>
/// <code>
/// var ability = new FireballAbility();
/// var context = new AbilityContext(handle, eventData);
/// ability.ActivateAbility(context);
/// </code>
/// </example>
public bool TryActivateAbility(AbilityContext context)
{
    // ...
}
```

### DocFX 빌드

```bash
cd UpmPackages/com.noname.gameabilitysystem.plugin
docfx docfx.json --serve
# http://localhost:8080
```

---

## 참고 자료

- **[Unreal GAS Documentation](https://dev.epicgames.com/documentation/en-us/unreal-engine/gameplay-ability-system-for-unreal-engine)**
- **[Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)**
- **[DocFX Documentation](https://dotnet.github.io/docfx/)**
