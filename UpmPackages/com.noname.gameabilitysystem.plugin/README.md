# GameAbilitySystem Plugin

Unity용 게임플레이 어빌리티 시스템 플러그인입니다. GAS (Gameplay Ability System) 패턴을 기반으로 하여 스킬, 효과, 속성, 태그 시스템을 제공합니다.

## 특징

- ✅ **Clean Architecture**: Domain/Presentation 계층 분리
- ✅ **Host 환경 지원**: 순수 C# 모델로 멀티스레드 환경에서 사용 가능
- ✅ **ScriptableObject 기반**: 디자이너 친화적 데이터 입력
- ✅ **JSON 직렬화**: 런타임 데이터 로딩 지원
- ✅ **스레드 안전**: AbilitySystemModel은 lock 기반 동기화 제공
- ✅ **태그 시스템**: 계층적 게임플레이 태그
- ✅ **효과 스택**: Duration, Infinite, Instant 효과 지원
- ✅ **속성 수정자**: Add, Multiply, Override 연산

## 아키텍처

```
Runtime/
├── Domain/                    # 순수 C# (Host 사용 가능)
│   ├── Models/
│   │   ├── Ability/          # 런타임 데이터 모델
│   │   │   ├── GameplayAbilityModel.cs
│   │   │   ├── GameplayEffectModel.cs
│   │   │   ├── GameplayModifier.cs
│   │   │   └── ...
│   │   └── AbilitySystemModel.cs
│   └── Snapshots/
│       └── AbilitySystemSnapshot.cs
│
├── Presentation/              # Unity 의존
│   ├── Components/
│   │   └── AbilitySystemComponent.cs  # ViewModel 역할
│   └── Data/                  # ScriptableObject (디자이너 툴)
│       ├── GameplayAbilityDefinition.cs
│       ├── GameplayEffectConfig.cs
│       └── ...
│
└── Ability/                   # 실행 로직
    ├── GameplayAbility.cs     # 추상 클래스
    ├── GameplayAbilityInstance.cs
    ├── GameplayAbilitySpec.cs
    └── AbilityContext.cs
```

## 사용 환경

### 1. Unity 환경 (Presentation Layer)

```csharp
// AbilitySystemComponent 사용 (MonoBehaviour)
public class CharacterController : MonoBehaviour, IAbilitySystemProvider
{
    [SerializeField] private AbilitySystemComponent _abilitySystem;

    private void Start()
    {
        // ScriptableObject 기반 어빌리티 부여
        _abilitySystem.GiveAbility(fireball Ability);

        // 활성화
        _abilitySystem.TryActivateAbilityByType(typeof(FireballAbility));
    }
}
```

### 2. Host 환경 (Domain Layer)

```csharp
// 순수 C# 모델 사용 (멀티스레드 지원)
public class CombatHost
{
    private readonly AbilitySystemModel _abilityModel;

    public void ApplyDamage(GameplayAbilityModel ability)
    {
        // 스레드 안전하게 처리
        foreach (var effect in ability.AppliedEffects)
        {
            foreach (var group in effect.ModifierGroups)
            {
                foreach (var modifier in group.Modifiers)
                {
                    ApplyModifier(modifier);
                }
            }
        }
    }
}
```

## 핵심 개념

### GameplayAbility (실행 로직)
어빌리티의 동작을 정의하는 추상 클래스입니다.

```csharp
public class FireballAbility : GameplayAbility
{
    protected override void ActivateAbility(AbilityContext context)
    {
        // 어빌리티 실행 로직
        var targetData = context.TargetData;
        ApplyDamageToTargets(targetData);
    }
}
```

### GameplayAbilityModel (데이터)
어빌리티의 설정 정보를 담는 순수 C# 모델입니다.

```csharp
var abilityModel = new GameplayAbilityModel
{
    AbilityId = "Fireball",
    DisplayName = "파이어볼",
    Cooldown = 5f,
    AppliedEffects = new List<GameplayEffectModel>
    {
        new GameplayEffectModel
        {
            ModifierGroups = new List<GameplayModifierGroup>
            {
                new GameplayModifierGroup
                {
                    Modifiers = new List<GameplayModifier>
                    {
                        new GameplayModifier
                        {
                            AttributeName = "Damage",
                            ModifierType = ModifierOperationType.Add,
                            Value = 50f
                        }
                    }
                }
            }
        }
    }
};
```

### GameplayEffect (효과)
속성 수정, 태그 부여 등의 효과를 정의합니다.

```csharp
// Instant: 즉시 적용 후 사라짐
DurationType = EffectDurationType.Instant

// HasDuration: 일정 시간 후 만료
DurationType = EffectDurationType.HasDuration
Duration = 10f  // 10초

// Infinite: 수동으로 제거할 때까지 유지
DurationType = EffectDurationType.Infinite
```

### GameplayModifier (수정자)
속성 값을 변경하는 수정자입니다.

```csharp
// 더하기: CurrentValue + Value
ModifierType = ModifierOperationType.Add
Value = 10f  // +10

// 퍼센트 더하기: CurrentValue * (1 + Value/100)
ModifierType = ModifierOperationType.AddPercent
Value = 50f  // +50% (1.5배)

// 곱하기: CurrentValue * Value
ModifierType = ModifierOperationType.Multiply
Value = 2f   // 2배

// 덮어쓰기: Value
ModifierType = ModifierOperationType.Override
Value = 100f // 100으로 고정
```

### AbilitySystemModel (상태 관리)
스레드 안전한 어빌리티 시스템 상태 관리자입니다.

```csharp
var model = new AbilitySystemModel();

// 속성 관리 (스레드 안전)
model.Set(AttributeId.Health, 100f);
model.Add(AttributeId.Health, -20f);
model.AddPercent(AttributeId.Damage, 0.5f); // +50%

// 태그 관리
model.AddLooseTag(new FGameplayTag("Status.Burning"), out var count);
model.RemoveLooseTag(new FGameplayTag("Status.Burning"), out count);

// 스킬 관리
model.AddSkill("Fireball");
var skills = model.GetSkills();

// Snapshot (불변 복사본)
var snapshot = model.BuildSnapshot();
```

## 데이터 워크플로우

### Unity → Host 데이터 흐름

``` 추후 작업 예정..
1. ScriptableObject (디자이너 작업)
   GameplayAbilityDefinition.asset
   GameplayEffectConfig.asset

2. JSON 추출 (에디터 툴)
   export_abilities.json

3. Host에서 로딩
   var json = File.ReadAllText("abilities.json");
   var catalog = JsonUtility.FromJson<AbilityCatalog>(json);

4. 런타임 사용
   var abilityModel = catalog.Abilities[0];
   ApplyAbility(abilityModel);
```

## 폴더별 상세 가이드

- [Domain 폴더](Runtime/Domain/README.md) - 순수 C# 모델
- [Presentation 폴더](Runtime/Presentation/README.md) - Unity 컴포넌트
- [Ability 폴더](Runtime/Ability/README.md) - 실행 로직

## 예제

### 간단한 데미지 어빌리티

```csharp
// 1. 모델 생성
var damageEffect = new GameplayEffectModel
{
    EffectId = "BasicDamage",
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

var ability = new GameplayAbilityModel
{
    AbilityId = "BasicAttack",
    DisplayName = "기본 공격",
    AppliedEffects = new List<GameplayEffectModel> { damageEffect }
};

// 2. Host에서 사용
public void UseAbility(GameplayAbilityModel ability)
{
    foreach (var effect in ability.AppliedEffects)
    {
        ApplyEffect(effect);
    }
}
```

### 버프 효과

```csharp
var buffEffect = new GameplayEffectModel
{
    EffectId = "AttackBuff",
    DisplayName = "공격력 증가",
    DurationType = EffectDurationType.HasDuration,
    Duration = 30f,  // 30초 지속
    ModifierGroups = new List<GameplayModifierGroup>
    {
        new GameplayModifierGroup
        {
            Modifiers = new List<GameplayModifier>
            {
                new GameplayModifier("Attack", ModifierOperationType.AddPercent, 50f)  // +50%
            }
        }
    },
    GrantedTags = new List<string> { "Buff.Attack" }
};
```

## 스레드 안전성

### AbilitySystemModel
모든 public 메서드는 `lock`으로 보호됩니다:

```csharp
public void Set(AttributeId id, float value)
{
    lock (_modelLock)  // 자동으로 스레드 안전
    {
        // ...
    }
}
```

### Snapshot
불변 복사본으로 스레드 간 안전한 데이터 전달:

```csharp
// Host 스레드에서
var snapshot = _model.BuildSnapshot();

// Unity 메인 스레드에서
UpdateUI(snapshot);  // 안전하게 읽기
```
