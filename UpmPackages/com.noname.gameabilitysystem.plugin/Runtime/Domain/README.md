# Domain Layer

순수 C# 비즈니스 모델을 포함하는 계층입니다. **Unity에 의존하지 않으며**, Host 환경(멀티스레드)에서 사용 가능합니다.

## 구조

```
Domain/
├── Models/
│   ├── Ability/              # 어빌리티 관련 순수 C# 모델
│   │   ├── GameplayAbilityModel.cs
│   │   ├── GameplayEffectModel.cs
│   │   ├── GameplayModifier.cs
│   │   ├── GameplayModifierGroup.cs
│   │   ├── EffectDurationType.cs
│   │   ├── ModifierOperationType.cs
│   │   └── AbilityCatalog.cs
│   │
│   └── AbilitySystemModel.cs  # 스레드 안전 상태 관리자
│
└── Snapshots/
    └── AbilitySystemSnapshot.cs  # 불변 복사본
```

## 핵심 원칙

### 1. Unity 독립성
- ❌ `UnityEngine` 네임스페이스 사용 금지
- ❌ `MonoBehaviour`, `ScriptableObject` 사용 금지
- ✅ 순수 C# 클래스만 사용
- ✅ `[Serializable]` 속성으로 JSON 직렬화 지원

### 2. 스레드 안전성
- `AbilitySystemModel`은 `lock` 기반 동기화 제공
- Snapshot은 불변 복사본으로 스레드 간 안전한 데이터 전달

### 3. 데이터 중심 설계
- 실행 로직은 포함하지 않음 (Ability 폴더에 위치)
- 순수한 데이터 모델만 정의

## Models/Ability

### GameplayAbilityModel
어빌리티의 설정 정보를 담는 데이터 클래스입니다.

```csharp
public sealed class GameplayAbilityModel
{
    public string AbilityId { get; set; }          // "Fireball"
    public string DisplayName { get; set; }        // "파이어볼"
    public string Description { get; set; }        // "강력한 화염구를 발사합니다"
    public float Cooldown { get; set; }            // 5.0 (초)

    public List<GameplayEffectModel> CostEffects { get; set; }      // 비용
    public List<GameplayEffectModel> AppliedEffects { get; set; }   // 효과

    public List<string> ActivationRequiredTags { get; set; }  // 필수 태그
    public List<string> ActivationBlockedTags { get; set; }   // 차단 태그
}
```

**사용 예:**
```csharp
// Host 환경에서 직접 생성
var ability = new GameplayAbilityModel
{
    AbilityId = "QuickStrike",
    DisplayName = "빠른 일격",
    Cooldown = 0f,
    AppliedEffects = new List<GameplayEffectModel>
    {
        CreateDamageEffect(50f)
    }
};
```

### GameplayEffectModel
속성 수정, 태그 부여 등의 효과를 정의합니다.

```csharp
public sealed class GameplayEffectModel
{
    public string EffectId { get; set; }
    public string DisplayName { get; set; }
    public EffectDurationType DurationType { get; set; }  // Instant, Infinite, HasDuration
    public float Duration { get; set; }                   // HasDuration일 때만 사용
    public float Period { get; set; }                     // 주기적 효과
    public int MaxStack { get; set; }                     // 최대 스택 수

    public List<GameplayModifierGroup> ModifierGroups { get; set; }  // 수정자
    public List<string> GrantedTags { get; set; }                    // 부여 태그
    public List<string> RequiredTags { get; set; }                   // 필수 태그
    public List<string> BlockedTags { get; set; }                    // 차단 태그
}
```

**지속 타입:**
- `Instant`: 즉시 적용 후 사라짐 (데미지, 힐)
- `HasDuration`: 일정 시간 후 만료 (버프, 디버프)
- `Infinite`: 수동 제거 전까지 유지 (영구 패시브)

### GameplayModifier
속성 값을 변경하는 수정자입니다.

```csharp
public sealed class GameplayModifier
{
    public string AttributeName { get; set; }          // "Damage", "Health", "Speed"
    public ModifierOperationType ModifierType { get; set; }
    public float Value { get; set; }
}
```

**연산 타입:**
```csharp
public enum ModifierOperationType
{
    Add,         // CurrentValue + Value
    AddPercent,  // CurrentValue * (1 + Value/100)
    Multiply,    // CurrentValue * Value
    Override     // Value (덮어쓰기)
}
```

**예제:**
```csharp
// 데미지 +50% 증가
new GameplayModifier
{
    AttributeName = "Damage",
    ModifierType = ModifierOperationType.AddPercent,
    Value = 50f  // +50%
}

// 체력 -20 감소
new GameplayModifier
{
    AttributeName = "Health",
    ModifierType = ModifierOperationType.Add,
    Value = -20f
}
```

### GameplayModifierGroup
여러 수정자를 그룹화합니다.

```csharp
public sealed class GameplayModifierGroup
{
    public List<GameplayModifier> Modifiers { get; set; }
}
```

하나의 효과에 여러 수정자 그룹을 적용할 수 있습니다:
```csharp
var effect = new GameplayEffectModel
{
    ModifierGroups = new List<GameplayModifierGroup>
    {
        new GameplayModifierGroup
        {
            Modifiers = new List<GameplayModifier>
            {
                new GameplayModifier("Damage", ModifierOperationType.AddPercent, 50f),
                new GameplayModifier("Speed", ModifierOperationType.Multiply, 1.5f)
            }
        }
    }
};
```

### AbilityCatalog
여러 어빌리티를 하나의 카탈로그로 관리합니다. JSON 직렬화를 위해 사용됩니다.

```csharp
public sealed class AbilityCatalog
{
    public string Version { get; set; }                      // "1.0"
    public List<GameplayAbilityModel> Abilities { get; set; }
}
```

**JSON 예제:**
```json
{
  "Version": "1.0",
  "Abilities": [
    {
      "AbilityId": "Fireball",
      "DisplayName": "파이어볼",
      "Cooldown": 5.0,
      "AppliedEffects": [...]
    }
  ]
}
```

## Models/AbilitySystemModel

스레드 안전한 어빌리티 시스템 상태 관리자입니다.

### 주요 기능

#### 1. 속성 관리
```csharp
var model = new AbilitySystemModel();

// 설정
model.Set(AttributeId.Health, 100f);

// 증가/감소
model.Add(AttributeId.Health, -20f);  // 20 감소

// 퍼센트 증가
model.AddPercent(AttributeId.Damage, 0.5f);  // +50%

// 조회
float health = model.Get(AttributeId.Health);
```

#### 2. 태그 관리
```csharp
// 루즈 태그 추가/제거
model.AddLooseTag(new FGameplayTag("Status.Burning"), out var count);
model.RemoveLooseTag(new FGameplayTag("Status.Burning"), out count);

// 효과 태그 추가/제거 (자동 관리)
model.AddEffectTag(tag, out count);
model.RemoveEffectTag(tag, out count);

// 총 개수 조회
int totalCount = model.GetTotalTagCount(tag);
```

#### 3. 스킬 관리
```csharp
model.AddSkill("Fireball");
model.AddSkill("IceBolt");

var skills = model.GetSkills();  // 복사본 반환 (스레드 안전)
```

#### 4. 활성 효과 관리
```csharp
// 효과 추가
long uid = model.AddActiveEffect(effectConfig, endTime);

// 효과 제거
model.RemoveActiveEffectByUid(uid);
model.RemoveActiveEffect(effectConfig);

// 만료된 효과 수집
var expired = new List<GameplayEffectConfig>();
model.CollectExpiredEffects(Time.time, expired);

// 활성 효과 조회
var effects = model.GetActiveEffects();  // 복사본 반환
```

### 스레드 안전성

모든 public 메서드는 `_modelLock`으로 보호됩니다:

```csharp
public void Set(AttributeId id, float value)
{
    lock (_modelLock)
    {
        // 스레드 안전하게 처리
        if (_attributes.TryGet(id, out var attr))
        {
            attr.CurrentValue = value;
        }
        else
        {
            _fallbackValues[id] = value;
        }
    }
}
```

**주의:**
- Unsafe 메서드(`GetUnsafe`, `SetUnsafe`)는 lock 내부에서만 사용
- Snapshot 빌드는 lock 내부에서 수행되어 일관성 보장

## Snapshots/AbilitySystemSnapshot

불변 복사본으로 스레드 간 안전한 데이터 전달을 담당합니다.

```csharp
public sealed class AbilitySystemSnapshot
{
    public IReadOnlyDictionary<AttributeId, float> Attributes { get; }
    public IReadOnlyList<FGameplayTag> OwnedTags { get; }
    public IReadOnlyList<string> Skills { get; }
    public IReadOnlyList<ActiveGameplayEffectSnapshot> ActiveEffects { get; }
}
```

### 사용 예

```csharp
// Host 스레드에서 생성
var snapshot = _model.BuildSnapshot();

// Unity 메인 스레드로 전달
UpdateUI(snapshot);  // 안전하게 읽기 전용 접근

// 속성 조회
if (snapshot.Attributes.TryGetValue(AttributeId.Health, out var health))
{
    Debug.Log($"Health: {health}");
}

// 태그 확인
bool hasBurning = snapshot.OwnedTags.Any(t => t.Value == "Status.Burning");
```

## 사용 시나리오

### Host 환경에서 어빌리티 적용

```csharp
public class CombatHost
{
    private readonly AbilitySystemModel _targetModel;

    public void ApplyAbility(GameplayAbilityModel ability)
    {
        foreach (var effect in ability.AppliedEffects)
        {
            ApplyEffect(effect);
        }
    }

    private void ApplyEffect(GameplayEffectModel effect)
    {
        // 태그 조건 확인
        if (effect.RequiredTags.Count > 0)
        {
            // 필수 태그 체크
        }

        // 수정자 적용
        foreach (var group in effect.ModifierGroups)
        {
            foreach (var modifier in group.Modifiers)
            {
                ApplyModifier(modifier);
            }
        }

        // 효과 등록 (Duration 타입인 경우)
        if (effect.DurationType == EffectDurationType.HasDuration)
        {
            var endTime = Time.time + effect.Duration;
            _targetModel.AddActiveEffect(effectConfig, endTime);
        }

        // 태그 부여
        foreach (var tag in effect.GrantedTags)
        {
            _targetModel.AddEffectTag(new FGameplayTag(tag), out _);
        }
    }

    private void ApplyModifier(GameplayModifier modifier)
    {
        var attributeId = GetAttributeId(modifier.AttributeName);
        var current = _targetModel.Get(attributeId);

        float newValue = modifier.ModifierType switch
        {
            ModifierOperationType.Add => current + modifier.Value,
            ModifierOperationType.AddPercent => current * (1f + modifier.Value / 100f),
            ModifierOperationType.Multiply => current * modifier.Value,
            ModifierOperationType.Override => modifier.Value,
            _ => current
        };

        _targetModel.Set(attributeId, newValue);
    }
}
```

## 설계 철학

1. **불변성**: Snapshot은 복사본, 원본 Model은 lock 보호
2. **단순성**: 복잡한 로직은 Ability 계층에서 처리
3. **확장성**: 새로운 ModifierType, EffectType 추가 용이
4. **성능**: Fallback Dictionary로 동적 속성 지원

## 다음 단계

- [Presentation 폴더](../Presentation/README.md) - Unity 컴포넌트와 ScriptableObject
- [Ability 폴더](../Ability/README.md) - 실행 로직 구현
