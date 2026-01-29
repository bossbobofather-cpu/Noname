# Presentation Layer

Unity MonoBehaviour와 ScriptableObject로 구성된 표현 계층입니다.
Domain 모델을 감싸거나 변환하여 Unity 씬에서 사용할 수 있게 제공합니다.

## 구성
```
Presentation/
├── Components/   # AbilitySystemComponentAdapter 등
├── Ability/      # GameplayAbility 구현
├── Data/         # ScriptableObject Config
├── Tag/          # GameplayTagContainerView 등
├── Target/       # 타게팅 표현/데이터
├── Task/         # AbilityTask 구현
└── Utilities/    # 에디터/유틸리티
```

## 핵심 역할
- Domain 모델을 Unity 씬과 연결
- ScriptableObject 기반 설정 제공
- 이벤트/디버그 UI 연동
- 입력/타게팅/애니메이션 처리

## 사용 예시
```csharp
[SerializeField] private AbilitySystemComponentAdapter _abilitySystem;

void Start()
{
    _abilitySystem.TryActivateAbilityByType(typeof(MyAbility));
}
```

## 참고
- Domain 상태를 직접 수정하지 않고 Adapter/Config를 통해 반영합니다.
- 런타임 로직은 Domain에 있고, Presentation은 표현과 연결에 집중합니다.
