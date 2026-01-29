# GameAbilitySystem Plugin

Unity용 Gameplay Ability System(GAS) 스타일 플러그인입니다.
Domain(순수 C#)과 Presentation(Unity)을 분리해 Host 환경에서도 재사용할 수 있도록 설계했습니다.

## 구성 요약
- **Domain**: AbilitySystemComponent(모델), Attribute/Tag/Effect/Ability 등 순수 로직
- **Presentation**: Unity 컴포넌트/ScriptableObject/디버그 UI
- **Adapter**: Domain 모델을 Unity 표현으로 동기화하는 뷰/어댑터 계층

## 핵심 특징
- **스레드 안전 모델**: 락 기반 동기화
- **태그 기반 조건**: 활성/차단 태그로 능력 제어
- **효과/수정자 구조**: 지속/주기/즉시 효과 지원
- **타게팅 분리**: Target/Strategy 구조로 확장 가능

## 기본 사용 흐름
1. 태그/속성/효과/능력 정의를 ScriptableObject로 구성
2. AbilitySystemComponent(또는 Adapter)로 모델 생성/연결
3. 이벤트/입력으로 능력 활성화

```csharp
// Ability 부여
var handle = abilitySystem.GiveAbility(abilityDefinition);

// Ability 실행
abilitySystem.TryActivateAbility(handle);
```

## 폴더 구조
```
Runtime/
├── Domain/        # 순수 C# 모델/로직
├── Presentation/  # Unity 컴포넌트 및 SO/디버그
├── Tag/           # 태그 유틸/레지스트리
├── Target/        # 타게팅 데이터/전략
├── Task/          # AbilityTask 구현
└── Util/          # 공용 유틸
```

## 참고
- 도메인 레이어는 Unity 의존이 없어 Host/서버 환경에서도 사용 가능합니다.
- Presentation 레이어는 Unity 오브젝트와 에디터 기능을 포함합니다.

자세한 사용 방법과 설계는 문서 사이트를 참고하세요.
