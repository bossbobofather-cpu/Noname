# Ability Layer

Ability 실행 로직이 위치하는 Presentation 하위 계층입니다.  
`GameplayAbility`를 상속해 실제 능력 동작을 구현합니다.

## 구성
```
Ability/
├── GameplayAbility.cs
├── GameplayAbilitySpec.cs
├── GameplayAbilityInstance.cs
└── AbilityContext.cs
```

## 기본 사용 흐름
1. AbilityDefinition을 생성해 Ability를 등록
2. AbilitySystemComponent로 Ability를 부여
3. `TryActivateAbility`로 실행

```csharp
public sealed class BasicAttackAbility : GameplayAbility
{
    protected override void ActivateAbility(AbilityContext context)
    {
        // 타겟 판정 및 효과 적용
    }
}
```

## 팁
- 조건 확인은 `CanActivateAbility`에서 처리
- 초기화는 `OnInit`에서 처리
- 종료 처리는 `EndAbility`/`CancelAbility`에서 정리
