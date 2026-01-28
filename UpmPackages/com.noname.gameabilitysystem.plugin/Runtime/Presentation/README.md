# Presentation Layer

Unity MonoBehaviour와 ScriptableObject로 구성된 표현 계층입니다.  
Domain 모델을 감싸거나 변환하여 Unity 씬에서 사용할 수 있게 제공합니다.

## 구성
```
Presentation/
├── Components/   # AbilitySystemComponent
├── Ability/      # GameplayAbility 구현
├── Data/         # ScriptableObject Config
└── Tag/          # GameplayTagContainer 등
```

## 핵심 역할
- Domain 모델을 Unity 씬과 연결
- ScriptableObject 기반 설정 제공
- 이벤트/디버그 UI 연동

## 사용 예시
```csharp
[SerializeField] private AbilitySystemComponent _abilitySystem;

void Start()
{
    _abilitySystem.TryActivateAbilityByType(typeof(MyAbility));
}
```
