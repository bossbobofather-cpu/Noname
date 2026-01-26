# Presentation Layer

Unity에 의존하는 프레젠테이션 계층입니다. MonoBehaviour 컴포넌트와 ScriptableObject를 포함합니다.

## 구조

```
Presentation/
├── Components/
│   └── AbilitySystemComponent.cs  # ViewModel (MonoBehaviour)
│
└── Data/                           # ScriptableObject (디자이너 툴)
    ├── GameplayConfig.cs           # 추상 베이스 클래스
    ├── GameplayAbilityDefinition.cs
    ├── GameplayEffectConfig.cs
    ├── GameplayTagConfig.cs
    ├── GameplayTargetConfig.cs
    ├── GameplayTargetActorConfig.cs
    └── GameplayEventTriggerConfig.cs
```

## Components/AbilitySystemComponent

AbilitySystemModel을 래핑하는 ViewModel 역할의 MonoBehaviour입니다.

### 주요 역할

1. **ViewModel**: Domain Model과 Unity View 사이의 중재자
2. **이벤트 발행**: 속성 변경, 태그 추가/제거 이벤트
3. **생명주기 관리**: 어빌리티/효과의 Unity 생명주기 연동
4. **디버깅**: Inspector에서 실시간 상태 확인

### 기본 사용법

```csharp
public class CharacterController : MonoBehaviour, IAbilitySystemProvider
{
    [SerializeField] private AbilitySystemComponent _abilitySystem;
    [SerializeField] private GameplayAbilityDefinition _fireballAbility;

    private void Start()
    {
        // 어빌리티 부여
        var handle = _abilitySystem.GiveAbility(_fireballAbility);

        // 어빌리티 활성화
        _abilitySystem.TryActivateAbility(handle);
    }
}
```

### 주요 메서드

#### 어빌리티 부여/제거

```csharp
// ScriptableObject 기반 부여
FGameplayAbilitySpecHandle GiveAbility(GameplayAbilityDefinition definition)

// 인스턴스 기반 부여
FGameplayAbilitySpecHandle GiveAbility(GameplayAbility ability)

// 제거
bool RemoveAbility(GameplayAbilityDefinition definition)
bool RemoveAbility(FGameplayAbilitySpecHandle handle)
bool RemoveAbilityByType(Type abilityType)
```

#### 어빌리티 활성화

```csharp
// 핸들로 활성화
bool TryActivateAbility(FGameplayAbilitySpecHandle handle)

// 타입으로 활성화
bool TryActivateAbilityByType(Type abilityType)

// 정의로 활성화
bool TryActivateAbility(GameplayAbilityDefinition definition)

// 태그로 활성화 (여러 개 가능)
bool TryActivateAbilityByTag(FGameplayTag abilityTag)
```

#### 어빌리티 종료

```csharp
bool EndAbility(FGameplayAbilitySpecHandle handle)
bool EndAbilityByType(Type abilityType)
bool EndAbility(GameplayAbilityDefinition definition)
```

#### 태그 관리

```csharp
// 루즈 태그 추가/제거
void AddLooseTag(FGameplayTag tag)
void RemoveLooseTag(FGameplayTag tag)

// 태그 조회
GameplayTagContainer OwnedTags { get; }
```

#### 효과 적용

```csharp
// 효과 적용
void ApplyGameplayEffect(GameplayEffectConfig effectConfig)
void ApplyGameplayEffect(GameplayEffectConfig effectConfig, GameplayEffectContext context)

// 효과 제거
bool RemoveGameplayEffect(GameplayEffectConfig effectConfig)

// 활성 효과 조회
void GetActiveEffects(List<GameplayEffectConfig> results)
```

#### 게임플레이 이벤트 처리

```csharp
bool HandleGameplayEvent(GameplayEventData eventData)
```

### 이벤트

```csharp
// 속성 변경 시
_abilitySystem.onChangedAttributeModifier += (asc, modifier, oldValue, newValue) =>
{
    Debug.Log($"{modifier.Attribute.name}: {oldValue.CurrentValue} → {newValue.CurrentValue}");
};

// 태그 추가 시
_abilitySystem.onAddedTag += (asc, tag) =>
{
    Debug.Log($"Tag Added: {tag.Value}");
};

// 태그 제거 시
_abilitySystem.onRemovedTag += (asc, tag) =>
{
    Debug.Log($"Tag Removed: {tag.Value}");
};

// 게임플레이 이벤트 발생 시
_abilitySystem.onGameplayEvent += (asc, eventData) =>
{
    Debug.Log($"Event: {eventData.EventTag.Value}");
};
```

### 프로퍼티

```csharp
// 속성 집합
AttributeSet Attributes { get; }

// 소유 태그
GameplayTagContainer OwnedTags { get; }

// 부여된 어빌리티 목록
IReadOnlyList<GameplayAbilitySpec> Abilities { get; }

// 소유자 컴포넌트
Component Owner { get; }

// 내부 모델
AbilitySystemModel Model { get; }
```

## Data (ScriptableObjects)

디자이너가 Unity Inspector에서 데이터를 입력할 수 있는 ScriptableObject 에셋입니다.

### GameplayAbilityDefinition

어빌리티 정의 에셋입니다.

```csharp
[CreateAssetMenu(menuName = "GameAbilitySystem/Config/GameplayAbilityDefinition")]
public sealed class GameplayAbilityDefinition : ScriptableObject
{
    [SerializeField] private string _abilityTypeName;  // "MyGame.FireballAbility"
    [SerializeField] private List<GameplayConfig> _configs;

    public string AbilityTypeName { get; }
    public IReadOnlyList<GameplayConfig> Configs { get; }
}
```

**사용 예:**
1. Create → GameAbilitySystem → Config → GameplayAbilityDefinition
2. Ability Type Name: 어빌리티 클래스 전체 이름 입력
3. Configs: 필요한 Config 에셋 추가 (TagConfig, EffectConfig 등)

### GameplayEffectConfig

효과 설정 에셋입니다.

```csharp
[CreateAssetMenu(menuName = "GameAbilitySystem/Config/GameplayEffectConfig")]
public class GameplayEffectConfig : GameplayConfig
{
    [SerializeField] private EGameplayEffectDurationType _durationType;
    [SerializeField] private float _duration;
    [SerializeField] private float _period;
    [SerializeField] private GameplayTagContainer _grantedTags;
    [SerializeField] private GameplayTagContainer _activationRequiredTags;
    [SerializeField] private GameplayTagContainer _activationBlockedTags;
    [SerializeField] private List<AttributeModifier> _modifiers;

    public EGameplayEffectDurationType DurationType { get; }
    public float Duration { get; }
    public GameplayTagContainer GrantedTags { get; }
    public IReadOnlyList<AttributeModifier> Modifiers { get; }
}
```

**Duration Type:**
- `Instant`: 즉시 적용 후 사라짐
- `HasDuration`: Duration만큼 지속
- `Infinite`: 수동 제거 전까지 유지

**Inspector 설정:**
1. Duration Type 선택
2. Modifiers 추가 (Attribute, Operation, Magnitude)
3. Granted Tags 설정 (이 효과가 부여하는 태그)
4. Required/Blocked Tags 설정 (적용 조건)

### GameplayTagConfig

어빌리티의 태그 설정 에셋입니다.

```csharp
[CreateAssetMenu(menuName = "GameAbilitySystem/Config/GameplayTagConfig")]
public class GameplayTagConfig : GameplayConfig
{
    [SerializeField] private GameplayTagContainer _abilityTags;
    [SerializeField] private GameplayTagContainer _activationRequiredTags;
    [SerializeField] private GameplayTagContainer _activationBlockedTags;

    public GameplayTagContainer AbilityTags { get; }
    public GameplayTagContainer ActivationRequiredTags { get; }
    public GameplayTagContainer ActivationBlockedTags { get; }
}
```

**사용 예:**
- Ability Tags: 이 어빌리티가 가진 태그 (예: "Ability.Fire", "Ability.Attack")
- Activation Required Tags: 활성화에 필요한 태그 (예: "State.Alive")
- Activation Blocked Tags: 활성화를 막는 태그 (예: "State.Stunned")

### GameplayEventTriggerConfig

이벤트 트리거 설정 에셋입니다.

```csharp
[CreateAssetMenu(menuName = "GameAbilitySystem/Config/GameplayEventTriggerConfig")]
public class GameplayEventTriggerConfig : GameplayConfig
{
    public FGameplayTag TriggerTag;        // 트리거할 태그
    public bool ActivateOnEvent = true;    // 이벤트 수신 시 즉시 활성화
}
```

**사용 예:**
```csharp
// 이벤트 발생
var eventData = new GameplayEventData
{
    EventTag = new FGameplayTag("Event.PlayerDamaged"),
    Instigator = attacker,
    Target = this
};

_abilitySystem.HandleGameplayEvent(eventData);
// → TriggerTag가 "Event.PlayerDamaged"인 어빌리티가 자동 활성화됨
```

### GameplayTargetConfig

타겟팅 설정 에셋입니다.

```csharp
[CreateAssetMenu(menuName = "GameAbilitySystem/Config/GameplayTargetConfig")]
public sealed class GameplayTargetConfig : GameplayConfig
{
    [SerializeField] private GameplayTargetActorConfig _targetActorConfig;
    [SerializeField] private TargetConfirmationMode _confirmationMode;
    [SerializeField] private GameObject _reticlePrefab;
    [SerializeField] private List<GameplayEffectConfig> _effects;

    public TargetConfirmationMode ConfirmationMode { get; }
    public IReadOnlyList<GameplayEffectConfig> Effects { get; }
}
```

**Confirmation Mode:**
- `Instant`: 즉시 타겟 확정
- `UserConfirmed`: 사용자 확인 필요 (예: 마우스 클릭)

## 워크플로우

### 1. 디자이너 작업 (Unity Editor)

```
1. GameplayEffectConfig 생성
   - Create → GameAbilitySystem → Config → GameplayEffectConfig
   - Duration Type: Instant
   - Modifiers 추가: Health, Add, -50

2. GameplayTagConfig 생성
   - Ability Tags: "Ability.Attack"
   - Activation Required Tags: "State.Alive"

3. GameplayAbilityDefinition 생성
   - Ability Type Name: "MyGame.BasicAttackAbility"
   - Configs에 위 두 에셋 추가

4. 캐릭터에 AbilitySystemComponent 추가
   - Startup Ability Definitions에 위 Definition 추가
```

### 2. 프로그래머 작업 (코드)

```csharp
// 1. 어빌리티 클래스 구현
public class BasicAttackAbility : GameplayAbility
{
    protected override void ActivateAbility(AbilityContext context)
    {
        // 타겟에게 데미지 적용
        var target = context.TargetData?.TargetActor;
        if (target != null)
        {
            var targetASC = target.GetComponent<AbilitySystemComponent>();

            // Effect는 Definition의 Config에서 자동 적용됨
            Debug.Log("Attack!");
        }
    }
}

// 2. 런타임에서 사용
private void OnAttackButtonClicked()
{
    _abilitySystem.TryActivateAbilityByType(typeof(BasicAttackAbility));
}
```

### 3. Host 환경으로 데이터 전달 (선택사항)

```csharp
// JSON 추출 (에디터 툴)
var catalog = new AbilityCatalog();
foreach (var definition in abilityDefinitions)
{
    var model = ConvertToModel(definition);
    catalog.Abilities.Add(model);
}

var json = JsonUtility.ToJson(catalog, prettyPrint: true);
File.WriteAllText("abilities.json", json);

// Host에서 로딩
var json = File.ReadAllText("abilities.json");
var catalog = JsonUtility.FromJson<AbilityCatalog>(json);
var abilityModel = catalog.Abilities.Find(a => a.AbilityId == "BasicAttack");
```

## ViewModel 패턴

AbilitySystemComponent는 MVVM 패턴의 ViewModel 역할을 합니다:

```
Model (Domain)
  ↑ wraps
AbilitySystemComponent (ViewModel)
  ↑ binds
UI (View)
```

**데이터 흐름:**
1. Model 변경 → Component 이벤트 발행 → UI 갱신
2. UI 입력 → Component 메서드 호출 → Model 변경

**예제:**
```csharp
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private AbilitySystemComponent _abilitySystem;
    [SerializeField] private Slider _healthBar;

    private void OnEnable()
    {
        _abilitySystem.onChangedAttributeModifier += OnAttributeChanged;
    }

    private void OnDisable()
    {
        _abilitySystem.onChangedAttributeModifier -= OnAttributeChanged;
    }

    private void OnAttributeChanged(AbilitySystemComponent asc, AttributeModifier modifier, AttributeValue oldValue, AttributeValue newValue)
    {
        if (modifier.Attribute.name == "Health")
        {
            _healthBar.value = newValue.CurrentValue / newValue.MaxValue;
        }
    }
}
```

## 디버깅 팁

### Inspector에서 확인
- Owned Tags: 현재 소유 중인 태그
- Abilities: 부여된 어빌리티 목록
- Attributes: 속성 값 실시간 확인

### 시스템 메시지 출력
```csharp
// AbilitySystemComponent 설정
[SerializeField] private bool _emitSystemMessages = true;

// 자동으로 SystemMessageBus에 메시지 발행
// "어빌리티 부착: Fireball"
// "어빌리티 활성화: Fireball"
// "속성 수정: Health 100 → 80"
```

### 로그 활성화
```csharp
// Debug 빌드에서만 상세 로그 출력
#if UNITY_EDITOR || DEVELOPMENT_BUILD
Debug.Log($"Activating Ability: {spec.AbilityName}");
#endif
```

## 다음 단계

- [Domain 폴더](../Domain/README.md) - 순수 C# 모델
- [Ability 폴더](../Ability/README.md) - 실행 로직 구현
