# GameAbilitySystem Plugin

Unity용 Gameplay Ability System(GAS) 스타일 플러그인입니다.  
Domain(순수 C#)과 Presentation(Unity) 레이어를 분리하여 Host 환경에서도 사용할 수 있도록 설계했습니다.

## 구성 요약
- **Domain**: AbilitySystemModel, Attribute/Tag/Effect 모델 (Unity 의존 없음)
- **Presentation**: AbilitySystemComponent, ScriptableObject 기반 설정
- **Bridge**: Domain <-> Presentation 변환 및 동기화

## 특징
- Thread-Safe 모델 (lock 기반)
- Tag 기반 활성/차단 조건
- Effect/Modifier 구조
- Host 시뮬레이션 지원

## 폴더 구조
```
Runtime/
├── Domain/          # 순수 C# 모델
├── Presentation/    # Unity 컴포넌트 및 SO
└── Editor/          # 에디터 유틸
```

## 간단 사용 예시
```csharp
// Ability 부여
var handle = abilitySystem.GiveAbility(abilityDefinition);

// Ability 실행
abilitySystem.TryActivateAbility(handle);
```

자세한 내용은 하위 README를 참고하세요.
