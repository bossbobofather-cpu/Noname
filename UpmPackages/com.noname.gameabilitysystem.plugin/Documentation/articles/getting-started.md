# 시작하기

## 설치

### Unity 패키지 매니저(로컬)

1. Unity 프로젝트를 엽니다.
2. Package Manager를 엽니다. (Window > Package Manager)
3. **+** 버튼을 누르고 **Add package from disk...**를 선택합니다.
4. `UpmPackages/com.noname.gameabilitysystem.plugin/package.json` 경로로 이동합니다.
5. **Open**을 클릭합니다.

### 수동 설치

`com.noname.gameabilitysystem.plugin` 폴더를 프로젝트의 `Packages/` 디렉터리에 복사합니다.

---

## 첫 번째 Ability 만들기

### 단계 1: Ability 클래스 만들기

`GameplayAbility`를 상속받는 C# 스크립트를 생성합니다.

```csharp
using Noname.GameAbilitySystem.Presentation;
using UnityEngine;

public class BasicAttackAbility : GameplayAbility
{
    protected override void ActivateAbility(AbilityContext context)
    {
        Debug.Log("Basic Attack activated!");

        // 데미지 효과는 ScriptableObject 설정을 기반으로 자동 적용

        EndAbility(context.Handle);
    }

    public override bool CanActivateAbility()
    {
        // 공격 가능 여부 체크
        return !ASC.OwnedTags.HasTag(new FGameplayTag("Status.Stunned"));
    }
}
```

### 단계 2: ScriptableObject 설정 만들기

#### 2.1 Gameplay Tag Config 생성

1. Project 창에서 우클릭
2. **Create > GameAbilitySystem > Config > Gameplay Tag Config**
3. 이름을 `TagConfig_BasicAttack`으로 지정
4. Ability Tags에 `Ability.Attack.Basic` 추가

#### 2.2 Gameplay Effect Config 생성

1. **Create > GameAbilitySystem > Config > Gameplay Effect Config**
2. 이름을 `Effect_BasicAttackDamage`로 지정
3. 설정값 입력:
   - **Duration Type**: Instant
   - **Modifiers**: 1개 추가
     - Attribute: Health
     - Operation: Add
     - Magnitude: -10

#### 2.3 Ability Definition 생성

1. **Create > GameAbilitySystem > Ability Definition**
2. 이름을 `AbilityDef_BasicAttack`으로 지정
3. **Ability Type Name**: `BasicAttackAbility`
4. **Configs** 배열에 다음을 추가:
   - TagConfig_BasicAttack
   - Effect_BasicAttackDamage

### 단계 3: GameObject에 연결

```csharp
using Noname.GameAbilitySystem.Presentation;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private AbilitySystemComponent _abilitySystem;
    [SerializeField] private GameplayAbilityDefinition _basicAttackAbility;

    private void Start()
    {
        // 속성 초기화
        _abilitySystem.Attributes.SetAttribute(
            AttributeDefinition.Health,
            baseValue: 100f,
            minValue: 0f,
            maxValue: 100f
        );

        // 능력 부여
        _abilitySystem.GiveAbility(_basicAttackAbility);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _abilitySystem.TryActivateAbilityByType<BasicAttackAbility>();
        }
    }
}
```

### 단계 4: 컴포넌트 연결

1. Player GameObject에 `AbilitySystemComponent` 추가
2. `PlayerController` 스크립트 추가
3. 인스펙터에서 참조 연결
   - Ability System → AbilitySystemComponent
   - Basic Attack Ability → AbilityDef_BasicAttack

### 단계 5: 테스트

Play 후 **Space**를 눌러 능력이 실행되는지 확인합니다.

---

## 고급 예시: 타겟 선택 기반 Fireball

```csharp
using Noname.GameAbilitySystem.Presentation;
using UnityEngine;

public class FireballAbility : GameplayAbility
{
    [SerializeField] private TargetAcquisitionConfig _targetConfig;

    protected override void ActivateAbility(AbilityContext context)
    {
        // 타겟 선택 대기
        var targetTask = AbilityTask_WaitTargetData.Create(this, _targetConfig);
        targetTask.ValidData += OnTargetAcquired;
        targetTask.Cancelled += () => CancelAbility(context.Handle);
        targetTask.Activate();
    }

    private void OnTargetAcquired(AbilityTargetData targetData)
    {
        // 타겟 AbilitySystemComponent 가져오기
        var target = targetData.TargetActor.GetComponent<AbilitySystemComponent>();

        if (target != null)
        {
            // 데미지 효과 적용
            target.ApplyGameplayEffect(damageEffect);

            // 지속 피해 효과 적용
            target.ApplyGameplayEffect(burningEffect);
        }

        EndAbility(TaskOwner.Handle);
    }

    public override bool CanActivateAbility()
    {
        if (!ASC.Attributes.TryGet(AttributeId.Mana, out var mana))
            return false;

        return mana.CurrentValue >= 50f;
    }
}
```

---

## 핵심 개념

### 속성(Attributes)

체력/마나/공격력 같은 수치입니다.

```csharp
// 설정
ASC.Attributes.SetAttribute(AttributeId.Health, 100f, 0f, 100f);

// 조회
if (ASC.Attributes.TryGet(AttributeId.Health, out var health))
{
    Debug.Log($"Health: {health.CurrentValue}/{health.MaxValue}");
}

// 수정
ASC.Attributes.Modify(AttributeId.Health, -10f, ModifierOperationType.Add);
```

### 태그(Gameplay Tags)

조건 체크와 상태 표현을 위한 계층형 태그입니다.

```csharp
// 태그 추가
ASC.OwnedTags.AddTag(new FGameplayTag("Status.Poisoned"));

// 태그 체크
if (ASC.OwnedTags.HasTag(new FGameplayTag("Status.Poisoned")))
{
    Debug.Log("Character is poisoned!");
}

// 상위 태그 체크
if (ASC.OwnedTags.HasTag(new FGameplayTag("Status")))
{
    Debug.Log("Character has some status effect!");
}
```

### 효과(Gameplay Effects)

속성 변화/버프/디버프를 표현합니다.

```csharp
// 즉시 효과 (데미지)
var damageEffect = ScriptableObject.CreateInstance<GameplayEffectConfig>();
damageEffect.DurationType = EGameplayEffectDurationType.Instant;
ASC.ApplyGameplayEffect(damageEffect);

// 지속 효과 (버프)
var buffEffect = ScriptableObject.CreateInstance<GameplayEffectConfig>();
buffEffect.DurationType = EGameplayEffectDurationType.HasDuration;
buffEffect.Duration = 10f;
ASC.ApplyGameplayEffect(buffEffect);

// 무한 효과
var permBuffEffect = ScriptableObject.CreateInstance<GameplayEffectConfig>();
permBuffEffect.DurationType = EGameplayEffectDurationType.Infinite;
var handle = ASC.ApplyGameplayEffect(permBuffEffect);

// 제거
ASC.RemoveActiveEffect(handle);
```

---

## 다음 단계

- 아키텍처 가이드(architecture.md)
- 성능 가이드(performance.md)
- API 레퍼런스(../obj/api/index.md)

---

## 자주 발생하는 문제

### Ability가 실행되지 않을 때

1. `CanActivateAbility()`가 true인지 확인
2. Required Tags가 있는지 확인
3. Blocked Tags가 없는지 확인
4. 콘솔 로그 확인

### Effect가 적용되지 않을 때

1. 대상에 `AbilitySystemComponent`가 있는지 확인
2. Effect의 Application Tag 조건 확인
3. Immunity 태그가 차단하는지 확인
4. 대상에 해당 Attribute가 있는지 확인

### 레퍼런스 누락

1. ScriptableObject 연결 확인
2. Ability Definition의 Type Name 확인
3. AbilitySystemComponent 초기화 여부 확인
